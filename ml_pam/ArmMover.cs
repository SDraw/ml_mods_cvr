using ABI.CCK.Components;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using RootMotion.FinalIK;
using System.Reflection;
using UnityEngine;

namespace ml_pam
{
    [DisallowMultipleComponent]
    class ArmMover : MonoBehaviour
    {
        const float c_offsetLimit = 0.5f;

        static readonly float[] ms_tposeMuscles = typeof(ABI_RC.Systems.IK.SubSystems.BodySystem).GetField("TPoseMuscles", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as float[];
        static readonly Vector4 ms_pointVector = new Vector4(0f, 0f, 0f, 1f);
        static readonly Quaternion ms_offsetRight = Quaternion.Euler(0f, 0f, 90f);
        static readonly Quaternion ms_offsetRightDesktop = Quaternion.Euler(0f, 270f, 0f);
        static readonly Quaternion ms_palmToLeft = Quaternion.Euler(0f, 0f, -90f);

        bool m_inVR = false;
        VRIK m_vrIK = null;
        Vector2 m_armWeight = Vector2.zero;
        Transform m_origRightHand = null;
        float m_playspaceScale = 1f;

        bool m_enabled = true;
        ArmIK m_armIK = null;
        Transform m_target = null;
        Transform m_rotationTarget = null;
        CVRPickupObject m_pickup = null;
        Matrix4x4 m_offset = Matrix4x4.identity;
        bool m_targetActive = false;

        // Unity events
        void Start()
        {
            m_inVR = Utils.IsInVR();

            m_target = new GameObject("ArmPickupTarget").transform;
            m_target.parent = PlayerSetup.Instance.GetActiveCamera().transform;
            m_target.localPosition = Vector3.zero;
            m_target.localRotation = Quaternion.identity;

            m_rotationTarget = new GameObject("RotationTarget").transform;
            m_rotationTarget.parent = m_target;
            m_rotationTarget.localPosition = new Vector3(c_offsetLimit * Settings.GrabOffset, 0f, 0f);
            m_rotationTarget.localRotation = Quaternion.identity;

            m_enabled = Settings.Enabled;

            Settings.EnabledChange += this.SetEnabled;
            Settings.GrabOffsetChange += this.SetGrabOffset;
        }

        void OnDestroy()
        {
            Settings.EnabledChange -= this.SetEnabled;
            Settings.GrabOffsetChange -= this.SetGrabOffset;
        }

        void Update()
        {
            if(m_enabled && !ReferenceEquals(m_pickup, null))
            {
                if(m_pickup != null)
                {
                    Matrix4x4 l_result = m_pickup.transform.GetMatrix() * m_offset;
                    m_target.position = l_result * ms_pointVector;
                }
                else
                    this.OnPickupDrop(m_pickup);
            }
        }

        // IK updates
        void OnIKPreUpdate()
        {
            m_armWeight.Set(m_vrIK.solver.rightArm.positionWeight, m_vrIK.solver.rightArm.rotationWeight);

            if(m_targetActive && (Mathf.Approximately(m_armWeight.x, 0f) || Mathf.Approximately(m_armWeight.y, 0f)))
            {
                m_vrIK.solver.rightArm.positionWeight = 1f;
                m_vrIK.solver.rightArm.rotationWeight = 1f;
            }
        }
        void OnIKPostUpdate()
        {
            m_vrIK.solver.rightArm.positionWeight = m_armWeight.x;
            m_vrIK.solver.rightArm.rotationWeight = m_armWeight.y;
        }

        // Settings
        void SetEnabled(bool p_state)
        {
            m_enabled = p_state;

            RefreshArmIK();
            if(m_enabled)
                RestorePickup();
            else
                RestoreVRIK();
        }

        void SetGrabOffset(float p_value)
        {
            if(m_rotationTarget != null)
                m_rotationTarget.localPosition = new Vector3(c_offsetLimit * m_playspaceScale * p_value, 0f, 0f);
        }

        // Game events
        internal void OnAvatarClear()
        {
            m_vrIK = null;
            m_origRightHand = null;
            m_armIK = null;
            m_targetActive = false;
        }

        internal void OnAvatarSetup()
        {
            // Recheck if user could switch to VR
            if(m_inVR != Utils.IsInVR())
            {
                m_target.parent = PlayerSetup.Instance.GetActiveCamera().transform;
                m_target.localPosition = Vector3.zero;
                m_target.localRotation = Quaternion.identity;
            }

            m_inVR = Utils.IsInVR();
            m_vrIK = PlayerSetup.Instance._animator.GetComponent<VRIK>();

            if(PlayerSetup.Instance._animator.isHuman)
            {
                Vector3 l_hipsPos = Vector3.zero;
                Transform l_hips = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Hips);
                if(l_hips != null)
                    l_hipsPos = l_hips.localPosition;
                
                HumanPose l_currentPose = new HumanPose();
                HumanPoseHandler l_poseHandler = null;

                if(!m_inVR)
                {
                    l_poseHandler = new HumanPoseHandler(PlayerSetup.Instance._animator.avatar, PlayerSetup.Instance._avatar.transform);
                    l_poseHandler.GetHumanPose(ref l_currentPose);

                    HumanPose l_tPose = new HumanPose
                    {
                        bodyPosition = l_currentPose.bodyPosition,
                        bodyRotation = l_currentPose.bodyRotation,
                        muscles = new float[l_currentPose.muscles.Length]
                    };
                    for(int i = 0; i < l_tPose.muscles.Length; i++)
                        l_tPose.muscles[i] = ms_tposeMuscles[i];

                    l_poseHandler.SetHumanPose(ref l_tPose);
                }

                Transform l_hand = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand);
                if(l_hand != null)
                    m_rotationTarget.localRotation = (ms_palmToLeft * (m_inVR ? ms_offsetRight : ms_offsetRightDesktop)) * (PlayerSetup.Instance._avatar.transform.GetMatrix().inverse * l_hand.GetMatrix()).rotation;

                if(m_vrIK == null)
                {
                    Transform l_chest = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.UpperChest);
                    if(l_chest == null)
                        l_chest = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Chest);
                    if(l_chest == null)
                        l_chest = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Spine);

                    m_armIK = PlayerSetup.Instance._avatar.AddComponent<ArmIK>();
                    m_armIK.solver.isLeft = false;
                    m_armIK.solver.SetChain(
                        l_chest,
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightShoulder),
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightUpperArm),
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightLowerArm),
                        l_hand,
                        PlayerSetup.Instance._animator.transform
                    );
                    m_armIK.solver.arm.target = m_rotationTarget;
                    m_armIK.solver.arm.positionWeight = 1f;
                    m_armIK.solver.arm.rotationWeight = 1f;
                    m_armIK.solver.IKPositionWeight = 0f;
                    m_armIK.solver.IKRotationWeight = 0f;
                    m_armIK.enabled = m_enabled;
                }
                else
                {
                    m_origRightHand = m_vrIK.solver.rightArm.target;
                    m_vrIK.solver.OnPreUpdate += this.OnIKPreUpdate;
                    m_vrIK.solver.OnPostUpdate += this.OnIKPostUpdate;
                }

                l_poseHandler?.SetHumanPose(ref l_currentPose);
                l_poseHandler?.Dispose();

                if(l_hips != null)
                    l_hips.localPosition = l_hipsPos;
            }

            if(m_enabled)
                RestorePickup();
        }

        internal void OnPickupGrab(CVRPickupObject p_pickup, ControllerRay p_ray, Vector3 p_hit)
        {
            if(p_ray == ViewManager.Instance.desktopControllerRay)
            {
                m_pickup = p_pickup;

                // Set offsets
                if(m_pickup.gripType == CVRPickupObject.GripType.Origin)
                {
                    if(m_pickup.ikReference != null)
                        m_offset = (m_pickup.transform.GetMatrix().inverse * m_pickup.ikReference.GetMatrix());
                    else
                    {
                        if(m_pickup.gripOrigin != null)
                            m_offset = m_pickup.transform.GetMatrix().inverse * m_pickup.gripOrigin.GetMatrix();
                    }
                }
                else
                    m_offset = m_pickup.transform.GetMatrix().inverse * Matrix4x4.Translate(p_hit);

                if(m_enabled)
                {
                    if((m_vrIK != null) && !m_targetActive)
                    {
                        m_vrIK.solver.rightArm.target = m_rotationTarget;
                        m_targetActive = true;
                    }

                    if(m_armIK != null)
                    {
                        m_armIK.solver.IKPositionWeight = 1f;
                        m_armIK.solver.IKRotationWeight = 1f;
                    }
                }
            }
        }

        internal void OnPickupDrop(CVRPickupObject p_pickup)
        {
            if(m_pickup == p_pickup)
            {
                m_pickup = null;

                if(m_enabled)
                {
                    RestoreVRIK();

                    if(m_armIK != null)
                    {
                        m_armIK.solver.IKPositionWeight = 0f;
                        m_armIK.solver.IKRotationWeight = 0f;
                    }
                }
            }
        }

        internal void OnPlayspaceScale(float p_relation)
        {
            m_playspaceScale = p_relation;
            SetGrabOffset(Settings.GrabOffset);
        }

        // Arbitrary
        void RestorePickup()
        {
            if((m_vrIK != null) && (m_pickup != null))
            {
                m_vrIK.solver.rightArm.target = m_rotationTarget;
                m_targetActive = true;
            }
            if((m_armIK != null) && (m_pickup != null))
            {
                m_armIK.solver.IKPositionWeight = 1f;
                m_armIK.solver.IKRotationWeight = 1f;
            }
        }

        void RestoreVRIK()
        {
            if((m_vrIK != null) && m_targetActive)
            {
                m_vrIK.solver.rightArm.target = m_origRightHand;
                m_targetActive = false;
            }
        }

        void RefreshArmIK()
        {
            if(m_armIK != null)
                m_armIK.enabled = m_enabled;
        }
    }
}

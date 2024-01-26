using ABI.CCK.Components;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using RootMotion.FinalIK;
using UnityEngine;

namespace ml_pam
{
    [DisallowMultipleComponent]
    class ArmMover : MonoBehaviour
    {
        enum HandState
        {
            Empty = 0,
            Pickup,
            Extended
        }
        struct IKInfo
        {
            public Vector4 m_armsWeights;
            public Transform m_leftHandTarget;
            public Transform m_rightHandTarget;
        }

        const float c_offsetLimit = 0.5f;
        const KeyCode c_leftKey = KeyCode.Q;
        const KeyCode c_rightKey = KeyCode.E;

        static readonly Vector4 ms_pointVector = new Vector4(0f, 0f, 0f, 1f);
        static readonly Quaternion ms_offsetLeft = Quaternion.Euler(270f, 90f, 0f);
        static readonly Quaternion ms_offsetRight = Quaternion.Euler(270f, 270f, 0f);

        bool m_inVR = false;
        VRIK m_vrIK = null;
        float m_armLength = 0f;
        float m_playspaceScale = 1f;

        bool m_enabled = true;
        IKInfo m_vrIKInfo;
        Transform m_rootLeft = null;
        Transform m_rootRight = null;
        Transform m_leftTarget = null;
        Transform m_rightTarget = null;
        ArmIK m_armIKLeft = null;
        ArmIK m_armIKRight = null;
        CVRPickupObject m_pickup = null;
        Matrix4x4 m_offset;
        HandState m_leftHandState = HandState.Empty;
        HandState m_rightHandState = HandState.Empty;

        // Unity events
        void Start()
        {
            m_inVR = Utils.IsInVR();

            m_rootLeft = new GameObject("[ArmPickupLeft]").transform;
            m_rootLeft.parent = PlayerSetup.Instance.GetActiveCamera().transform;
            m_rootLeft.localPosition = Vector3.zero;
            m_rootLeft.localRotation = Quaternion.identity;

            m_leftTarget = new GameObject("Target").transform;
            m_leftTarget.parent = m_rootLeft;
            m_leftTarget.localPosition = new Vector3(c_offsetLimit * -Settings.GrabOffset, 0f, 0f);
            m_leftTarget.localRotation = Quaternion.identity;

            m_rootRight = new GameObject("[ArmPickupRight]").transform;
            m_rootRight.parent = PlayerSetup.Instance.GetActiveCamera().transform;
            m_rootRight.localPosition = Vector3.zero;
            m_rootRight.localRotation = Quaternion.identity;

            m_rightTarget = new GameObject("Target").transform;
            m_rightTarget.parent = m_rootRight;
            m_rightTarget.localPosition = new Vector3(c_offsetLimit * Settings.GrabOffset, 0f, 0f);
            m_rightTarget.localRotation = Quaternion.identity;

            m_enabled = Settings.Enabled;

            Settings.EnabledChange += this.SetEnabled;
            Settings.GrabOffsetChange += this.SetGrabOffset;
            Settings.LeadingHandChange += this.OnLeadingHandChange;
            Settings.HandsExtensionChange += this.OnHandsExtensionChange;
        }

        void OnDestroy()
        {
            if(m_armIKLeft != null)
                Destroy(m_armIKLeft);
            m_armIKLeft = null;

            if(m_armIKRight != null)
                Destroy(m_armIKRight);
            m_armIKRight = null;

            if(m_rootLeft != null)
                Destroy(m_rootLeft);
            m_rootLeft = null;
            m_leftTarget = null;

            if(m_rootRight != null)
                Destroy(m_rootRight);
            m_rootRight = null;
            m_rightTarget = null;

            m_pickup = null;
            m_vrIK = null;

            Settings.EnabledChange -= this.SetEnabled;
            Settings.GrabOffsetChange -= this.SetGrabOffset;
            Settings.LeadingHandChange -= this.OnLeadingHandChange;
            Settings.HandsExtensionChange -= this.OnHandsExtensionChange;
        }

        void Update()
        {
            if(!ReferenceEquals(m_pickup, null) && (m_pickup == null))
                OnPickupDrop(m_pickup);

            switch(m_leftHandState)
            {
                case HandState.Empty:
                {
                    if(Settings.HandsExtension && Input.GetKeyDown(c_leftKey))
                    {
                        m_leftHandState = HandState.Extended;
                        m_rootLeft.localPosition = new Vector3(0f, 0f, m_armLength * m_playspaceScale);
                        SetArmActive(Settings.LeadHand.Left, true);
                    }
                }
                break;
                case HandState.Extended:
                {
                    if(Input.GetKeyUp(c_leftKey))
                    {
                        m_leftHandState = HandState.Empty;
                        SetArmActive(Settings.LeadHand.Left, false);
                    }
                }
                break;
                case HandState.Pickup:
                {
                    if(m_pickup != null)
                    {
                        Matrix4x4 l_result = m_pickup.transform.GetMatrix() * m_offset;
                        m_rootLeft.position = l_result * ms_pointVector;
                    }
                }
                break;
            }

            switch(m_rightHandState)
            {
                case HandState.Empty:
                {
                    if(Settings.HandsExtension && Input.GetKeyDown(c_rightKey))
                    {
                        m_rightHandState = HandState.Extended;
                        m_rootRight.localPosition = new Vector3(0f, 0f, m_armLength * m_playspaceScale);
                        SetArmActive(Settings.LeadHand.Right, true);
                    }
                }
                break;
                case HandState.Extended:
                {
                    if(Input.GetKeyUp(c_rightKey))
                    {
                        m_rightHandState = HandState.Empty;
                        SetArmActive(Settings.LeadHand.Right, false);
                    }
                }
                break;
                case HandState.Pickup:
                {
                    if(m_pickup != null)
                    {
                        Matrix4x4 l_result = m_pickup.transform.GetMatrix() * m_offset;
                        m_rootRight.position = l_result * ms_pointVector;
                    }
                }
                break;
            }
        }

        // VRIK updates
        void OnIKPreUpdate()
        {
            if(m_enabled)
            {
                if(m_leftHandState != HandState.Empty)
                {
                    m_vrIKInfo.m_leftHandTarget = m_vrIK.solver.leftArm.target;
                    m_vrIKInfo.m_armsWeights.x = m_vrIK.solver.leftArm.positionWeight;
                    m_vrIKInfo.m_armsWeights.y = m_vrIK.solver.leftArm.rotationWeight;

                    m_vrIK.solver.leftArm.positionWeight = 1f;
                    m_vrIK.solver.leftArm.rotationWeight = 1f;
                    m_vrIK.solver.leftArm.target = m_leftTarget;
                }
                if(m_rightHandState != HandState.Empty)
                {
                    m_vrIKInfo.m_rightHandTarget = m_vrIK.solver.rightArm.target;
                    m_vrIKInfo.m_armsWeights.z = m_vrIK.solver.rightArm.positionWeight;
                    m_vrIKInfo.m_armsWeights.w = m_vrIK.solver.rightArm.rotationWeight;

                    m_vrIK.solver.rightArm.positionWeight = 1f;
                    m_vrIK.solver.rightArm.rotationWeight = 1f;
                    m_vrIK.solver.rightArm.target = m_rightTarget;
                }
            }
        }
        void OnIKPostUpdate()
        {
            if(m_enabled)
            {
                if(m_leftHandState != HandState.Empty)
                {
                    m_vrIK.solver.leftArm.target = m_vrIKInfo.m_leftHandTarget;
                    m_vrIK.solver.leftArm.positionWeight = m_vrIKInfo.m_armsWeights.x;
                    m_vrIK.solver.leftArm.rotationWeight = m_vrIKInfo.m_armsWeights.y;
                }
                if(m_rightHandState != HandState.Empty)
                {
                    m_vrIK.solver.rightArm.target = m_vrIKInfo.m_rightHandTarget;
                    m_vrIK.solver.rightArm.positionWeight = m_vrIKInfo.m_armsWeights.z;
                    m_vrIK.solver.rightArm.rotationWeight = m_vrIKInfo.m_armsWeights.w;
                }
            }
        }

        // Settings
        void SetEnabled(bool p_state)
        {
            m_enabled = p_state;

            if(m_enabled)
            {
                if(m_leftHandState != HandState.Empty)
                    SetArmActive(Settings.LeadHand.Left, true);
                if(m_rightHandState != HandState.Empty)
                    SetArmActive(Settings.LeadHand.Right, true);

                OnHandsExtensionChange(Settings.HandsExtension);
            }
            else
                SetArmActive(Settings.LeadHand.Both, false, true);
        }

        void SetGrabOffset(float p_value)
        {
            if(m_leftTarget != null)
                m_leftTarget.localPosition = new Vector3(c_offsetLimit * m_playspaceScale * -p_value, 0f, 0f);
            if(m_rightTarget != null)
                m_rightTarget.localPosition = new Vector3(c_offsetLimit * m_playspaceScale * p_value, 0f, 0f);
        }

        void OnLeadingHandChange(Settings.LeadHand p_hand)
        {
            if(m_pickup != null)
            {
                if(m_leftHandState == HandState.Pickup)
                {
                    m_leftHandState = HandState.Empty;
                    SetArmActive(Settings.LeadHand.Left, false);
                }
                if(m_rightHandState == HandState.Pickup)
                {
                    m_rightHandState = HandState.Empty;
                    SetArmActive(Settings.LeadHand.Right, false);
                }

                switch(p_hand)
                {
                    case Settings.LeadHand.Left:
                        m_leftHandState = HandState.Pickup;
                        break;
                    case Settings.LeadHand.Right:
                        m_rightHandState = HandState.Pickup;
                        break;
                    case Settings.LeadHand.Both:
                    {
                        m_leftHandState = HandState.Pickup;
                        m_rightHandState = HandState.Pickup;
                    }
                    break;
                }

                SetArmActive(p_hand, true);
            }
        }

        void OnHandsExtensionChange(bool p_state)
        {
            if(m_enabled)
            {
                if(p_state)
                {
                    if((m_leftHandState == HandState.Empty) && Input.GetKey(c_leftKey))
                    {
                        m_leftHandState = HandState.Extended;
                        SetArmActive(Settings.LeadHand.Left, true);
                    }
                    if((m_rightHandState == HandState.Empty) && Input.GetKey(c_rightKey))
                    {
                        m_rightHandState = HandState.Extended;
                        SetArmActive(Settings.LeadHand.Right, true);
                    }
                }
                else
                {
                    if(m_leftHandState == HandState.Extended)
                    {
                        m_leftHandState = HandState.Empty;
                        SetArmActive(Settings.LeadHand.Left, false);
                    }
                    if(m_rightHandState == HandState.Extended)
                    {
                        m_rightHandState = HandState.Empty;
                        SetArmActive(Settings.LeadHand.Right, false);
                    }
                }
            }
        }

        // Game events
        internal void OnAvatarClear()
        {
            m_vrIK = null;
            m_armIKLeft = null;
            m_armIKRight = null;
            m_armLength = 0f;
        }

        internal void OnAvatarSetup()
        {
            // Recheck if user could switch to VR
            if(m_inVR != Utils.IsInVR())
            {
                m_rootLeft.parent = PlayerSetup.Instance.GetActiveCamera().transform;
                m_rootLeft.localPosition = Vector3.zero;
                m_rootLeft.localRotation = Quaternion.identity;

                m_rootRight.parent = PlayerSetup.Instance.GetActiveCamera().transform;
                m_rootRight.localPosition = Vector3.zero;
                m_rootRight.localRotation = Quaternion.identity;
            }
            m_inVR = Utils.IsInVR();

            if(!m_inVR && PlayerSetup.Instance._animator.isHuman)
            {
                m_vrIK = PlayerSetup.Instance._animator.GetComponent<VRIK>();

                TPoseHelper l_tpHelper = new TPoseHelper();
                l_tpHelper.Assign(PlayerSetup.Instance._animator);
                l_tpHelper.Apply();

                Transform l_leftHand = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftHand);
                if(l_leftHand != null)
                    m_leftTarget.localRotation = ms_offsetLeft * (PlayerSetup.Instance._avatar.transform.GetMatrix().inverse * l_leftHand.GetMatrix()).rotation;
                Transform l_rightHand = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand);
                if(l_rightHand != null)
                    m_rightTarget.localRotation = ms_offsetRight * (PlayerSetup.Instance._avatar.transform.GetMatrix().inverse * l_rightHand.GetMatrix()).rotation;

                if(m_vrIK == null)
                {
                    Transform l_chest = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.UpperChest);
                    if(l_chest == null)
                        l_chest = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Chest);
                    if(l_chest == null)
                        l_chest = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Spine);

                    m_armIKLeft = PlayerSetup.Instance._avatar.AddComponent<ArmIK>();
                    m_armIKLeft.solver.isLeft = true;
                    m_armIKLeft.solver.SetChain(
                        l_chest,
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftShoulder),
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftUpperArm),
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftLowerArm),
                        l_leftHand,
                        PlayerSetup.Instance._animator.transform
                    );
                    m_armIKLeft.solver.arm.target = m_leftTarget;
                    m_armIKLeft.solver.arm.positionWeight = 1f;
                    m_armIKLeft.solver.arm.rotationWeight = 1f;
                    m_armIKLeft.solver.IKPositionWeight = 0f;
                    m_armIKLeft.solver.IKRotationWeight = 0f;
                    m_armIKLeft.enabled = false;

                    m_armLength = m_armIKLeft.solver.arm.mag * 1.25f;

                    m_armIKRight = PlayerSetup.Instance._avatar.AddComponent<ArmIK>();
                    m_armIKRight.solver.isLeft = false;
                    m_armIKRight.solver.SetChain(
                        l_chest,
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightShoulder),
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightUpperArm),
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightLowerArm),
                        l_rightHand,
                        PlayerSetup.Instance._animator.transform
                    );
                    m_armIKRight.solver.arm.target = m_rightTarget;
                    m_armIKRight.solver.arm.positionWeight = 1f;
                    m_armIKRight.solver.arm.rotationWeight = 1f;
                    m_armIKRight.solver.IKPositionWeight = 0f;
                    m_armIKRight.solver.IKRotationWeight = 0f;
                    m_armIKRight.enabled = false;
                }
                else
                {
                    m_armLength = m_vrIK.solver.leftArm.mag * 1.25f;
                    m_vrIK.solver.OnPreUpdate += this.OnIKPreUpdate;
                    m_vrIK.solver.OnPostUpdate += this.OnIKPostUpdate;
                }

                l_tpHelper.Restore();
                l_tpHelper.Unassign();
            }

            SetEnabled(m_enabled);
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

                switch(Settings.LeadingHand)
                {
                    case Settings.LeadHand.Left:
                        m_leftHandState = HandState.Pickup;
                        break;
                    case Settings.LeadHand.Right:
                        m_rightHandState = HandState.Pickup;
                        break;
                    case Settings.LeadHand.Both:
                    {
                        m_leftHandState = HandState.Pickup;
                        m_rightHandState = HandState.Pickup;
                    }
                    break;
                }

                SetArmActive(Settings.LeadingHand, true);
            }
        }

        internal void OnPickupDrop(CVRPickupObject p_pickup)
        {
            if(m_pickup == p_pickup)
            {
                m_pickup = null;
                switch(Settings.LeadingHand)
                {
                    case Settings.LeadHand.Left:
                        m_leftHandState = HandState.Empty;
                        break;
                    case Settings.LeadHand.Right:
                        m_rightHandState = HandState.Empty;
                        break;
                    case Settings.LeadHand.Both:
                    {
                        m_leftHandState = HandState.Empty;
                        m_rightHandState = HandState.Empty;
                    }
                    break;
                }
                SetArmActive(Settings.LeadingHand, false);
            }
        }

        internal void OnPlayspaceScale(float p_relation)
        {
            m_playspaceScale = p_relation;
            SetGrabOffset(Settings.GrabOffset);
        }

        // Arbitrary
        void SetArmActive(Settings.LeadHand p_hand, bool p_state, bool p_forced = false)
        {
            if(m_enabled || p_forced)
            {
                if(((p_hand == Settings.LeadHand.Left) || (p_hand == Settings.LeadHand.Both)) && (m_armIKLeft != null))
                {
                    m_armIKLeft.enabled = m_enabled;
                    m_armIKLeft.solver.IKPositionWeight = (p_state ? 1f : 0f);
                    m_armIKLeft.solver.IKRotationWeight = (p_state ? 1f : 0f);
                }
                if(((p_hand == Settings.LeadHand.Right) || (p_hand == Settings.LeadHand.Both)) && (m_armIKRight != null))
                {
                    m_armIKRight.enabled = m_enabled;
                    m_armIKRight.solver.IKPositionWeight = (p_state ? 1f : 0f);
                    m_armIKRight.solver.IKRotationWeight = (p_state ? 1f : 0f);
                }
            }
        }
    }
}

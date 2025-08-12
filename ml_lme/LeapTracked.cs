using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Systems.GameEventSystem;
using ABI_RC.Systems.IK;
using RootMotion.FinalIK;
using System.Collections.Generic;
using UnityEngine;

namespace ml_lme
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(999999)]
    class LeapTracked : MonoBehaviour
    {
        enum PlaneType
        {
            OXZ,
            OYX
        }

        struct IKInfo
        {
            public Vector4 m_armsWeights;
            public Vector2 m_elbowsWeights;
            public Transform m_leftHandTarget;
            public Transform m_rightHandTarget;
            public Transform m_leftElbowTarget;
            public Transform m_rightElbowTarget;
        }

        struct RotationOffset
        {
            public Transform m_target;
            public Transform m_source;
            public Quaternion m_offset;

            public void Reset()
            {
                m_source = null;
                m_target = null;
                m_offset = Quaternion.identity;
            }
        }

        static readonly Quaternion ms_offsetLeft = Quaternion.Euler(0f, 90f, 0f);
        static readonly Quaternion ms_offsetRight = Quaternion.Euler(0f, 270f, 0f);

        static readonly (HumanBodyBones, bool)[] ms_fingers =
        {
            (HumanBodyBones.LeftThumbProximal, true),
            (HumanBodyBones.LeftThumbIntermediate, true),
            (HumanBodyBones.LeftThumbDistal, true),
            (HumanBodyBones.LeftIndexProximal, true),
            (HumanBodyBones.LeftIndexIntermediate, true),
            (HumanBodyBones.LeftIndexDistal, true),
            (HumanBodyBones.LeftMiddleProximal, true),
            (HumanBodyBones.LeftMiddleIntermediate, true),
            (HumanBodyBones.LeftMiddleDistal, true),
            (HumanBodyBones.LeftRingProximal, true),
            (HumanBodyBones.LeftRingIntermediate, true),
            (HumanBodyBones.LeftRingDistal, true),
            (HumanBodyBones.LeftLittleProximal, true),
            (HumanBodyBones.LeftLittleIntermediate, true),
            (HumanBodyBones.LeftLittleDistal, true),

            (HumanBodyBones.RightThumbProximal, false),
            (HumanBodyBones.RightThumbIntermediate, false),
            (HumanBodyBones.RightThumbDistal, false),
            (HumanBodyBones.RightIndexProximal, false),
            (HumanBodyBones.RightIndexIntermediate, false),
            (HumanBodyBones.RightIndexDistal, false),
            (HumanBodyBones.RightMiddleProximal, false),
            (HumanBodyBones.RightMiddleIntermediate, false),
            (HumanBodyBones.RightMiddleDistal, false),
            (HumanBodyBones.RightRingProximal, false),
            (HumanBodyBones.RightRingIntermediate, false),
            (HumanBodyBones.RightRingDistal, false),
            (HumanBodyBones.RightLittleProximal, false),
            (HumanBodyBones.RightLittleIntermediate, false),
            (HumanBodyBones.RightLittleDistal, false),
        };
        static readonly (HumanBodyBones, HumanBodyBones, bool)[] ms_fingersChains =
        {
            (HumanBodyBones.LeftThumbProximal,HumanBodyBones.LeftThumbIntermediate,true), (HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal,true), (HumanBodyBones.LeftThumbDistal,HumanBodyBones.LastBone,true),
            (HumanBodyBones.LeftIndexProximal,HumanBodyBones.LeftIndexIntermediate,true), (HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal,true), (HumanBodyBones.LeftIndexDistal,HumanBodyBones.LastBone,true),
            (HumanBodyBones.LeftMiddleProximal,HumanBodyBones.LeftMiddleIntermediate,true), (HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal,true), (HumanBodyBones.LeftMiddleDistal,HumanBodyBones.LastBone,true),
            (HumanBodyBones.LeftRingProximal,HumanBodyBones.LeftRingIntermediate,true), (HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal,true), (HumanBodyBones.LeftRingDistal,HumanBodyBones.LastBone,true),
            (HumanBodyBones.LeftLittleProximal,HumanBodyBones.LeftLittleIntermediate,true), (HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal,true), (HumanBodyBones.LeftLittleDistal,HumanBodyBones.LastBone,true),
            (HumanBodyBones.RightThumbProximal,HumanBodyBones.RightThumbIntermediate,false), (HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbDistal,false), (HumanBodyBones.RightThumbDistal,HumanBodyBones.LastBone,false),
            (HumanBodyBones.RightIndexProximal,HumanBodyBones.RightIndexIntermediate,false), (HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal,false), (HumanBodyBones.RightIndexDistal,HumanBodyBones.LastBone,false),
            (HumanBodyBones.RightMiddleProximal,HumanBodyBones.RightMiddleIntermediate,false), (HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal,false), (HumanBodyBones.RightMiddleDistal,HumanBodyBones.LastBone,false),
            (HumanBodyBones.RightRingProximal,HumanBodyBones.RightRingIntermediate,false), (HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal,false), (HumanBodyBones.RightRingDistal,HumanBodyBones.LastBone,false),
            (HumanBodyBones.RightLittleProximal,HumanBodyBones.RightLittleIntermediate,false), (HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal,false),(HumanBodyBones.RightLittleDistal,HumanBodyBones.LastBone,false),
        };

        static readonly Vector3[] ms_directions =
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            Vector3.up,
            Vector3.down,
        };

        public static readonly float[] ms_lastLeftFingerBones = new float[20];
        public static readonly float[] ms_lastRightFingerBones = new float[20];

        VRIK m_vrIK = null;
        Transform m_hips = null;

        IKInfo m_vrIKInfo;
        ArmIK m_leftArmIK = null;
        ArmIK m_rightArmIK = null;
        HumanPoseHandler m_poseHandler = null;
        HumanPose m_pose;
        Transform m_leftHandTarget = null;
        Transform m_rightHandTarget = null;
        bool m_leftTargetActive = false; // VRIK only
        bool m_rightTargetActive = false; // VRIK only

        RotationOffset m_leftHandOffset; // From avatar hand to Leap wrist
        RotationOffset m_rightHandOffset;
        readonly List<RotationOffset> m_leftFingerOffsets = null; // From Leap finger bone to avatar finger bone
        readonly List<RotationOffset> m_rightFingerOffsets = null;

        internal LeapTracked()
        {
            m_leftFingerOffsets = new List<RotationOffset>();
            m_rightFingerOffsets = new List<RotationOffset>();
        }

        // Unity events
        void Start()
        {
            m_leftHandTarget = new GameObject("RotationTarget").transform;
            m_leftHandTarget.parent = LeapTracking.Instance.GetLeftHand().GetRoot();
            m_leftHandTarget.localPosition = Vector3.zero;
            m_leftHandTarget.localRotation = Quaternion.identity;

            m_rightHandTarget = new GameObject("RotationTarget").transform;
            m_rightHandTarget.parent = LeapTracking.Instance.GetRightHand().GetRoot();
            m_rightHandTarget.localPosition = Vector3.zero;
            m_rightHandTarget.localRotation = Quaternion.identity;

            OnEnabledOrFingersOnlyChanged(Settings.Enabled || Settings.FingersOnly);
            OnTrackElbowsChanged(Settings.TrackElbows);

            Settings.OnEnabledChanged.AddListener(this.OnEnabledOrFingersOnlyChanged);
            Settings.OnFingersOnlyChanged.AddListener(this.OnEnabledOrFingersOnlyChanged);
            Settings.OnTrackElbowsChanged.AddListener(this.OnTrackElbowsChanged);

            CVRGameEventSystem.Avatar.OnLocalAvatarLoad.AddListener(this.OnAvatarSetup);
            CVRGameEventSystem.Avatar.OnLocalAvatarClear.AddListener(this.OnAvatarClear);
            GameEvents.OnAvatarReuse.AddListener(this.OnAvatarReuse);
        }

        void OnDestroy()
        {
            RemoveArmIK();

            if(m_leftHandTarget != null)
                Destroy(m_leftHandTarget);
            m_leftHandTarget = null;

            if(m_rightHandTarget != null)
                Destroy(m_rightHandTarget);
            m_rightHandTarget = null;

            m_poseHandler?.Dispose();
            m_poseHandler = null;

            m_vrIK = null;

            Settings.OnEnabledChanged.RemoveListener(this.OnEnabledOrFingersOnlyChanged);
            Settings.OnFingersOnlyChanged.RemoveListener(this.OnEnabledOrFingersOnlyChanged);
            Settings.OnTrackElbowsChanged.RemoveListener(this.OnTrackElbowsChanged);

            CVRGameEventSystem.Avatar.OnLocalAvatarLoad.RemoveListener(this.OnAvatarSetup);
            CVRGameEventSystem.Avatar.OnLocalAvatarClear.RemoveListener(this.OnAvatarClear);
            GameEvents.OnAvatarReuse.RemoveListener(this.OnAvatarReuse);
        }

        void Update()
        {
            if(Settings.Enabled)
            {
                LeapParser.LeapData l_data = LeapManager.Instance.GetLatestData();

                if((m_leftArmIK != null) && (m_rightArmIK != null))
                {
                    m_leftArmIK.solver.IKPositionWeight = Mathf.Lerp(m_leftArmIK.solver.IKPositionWeight, (l_data.m_leftHand.m_present && !Settings.FingersOnly) ? 1f : 0f, 0.25f);
                    m_leftArmIK.solver.IKRotationWeight = Mathf.Lerp(m_leftArmIK.solver.IKRotationWeight, (l_data.m_leftHand.m_present && !Settings.FingersOnly) ? 1f : 0f, 0.25f);
                    if(Settings.TrackElbows)
                        m_leftArmIK.solver.arm.bendGoalWeight = Mathf.Lerp(m_leftArmIK.solver.arm.bendGoalWeight, (l_data.m_leftHand.m_present && !Settings.FingersOnly) ? 1f : 0f, 0.25f);

                    m_rightArmIK.solver.IKPositionWeight = Mathf.Lerp(m_rightArmIK.solver.IKPositionWeight, (l_data.m_rightHand.m_present && !Settings.FingersOnly) ? 1f : 0f, 0.25f);
                    m_rightArmIK.solver.IKRotationWeight = Mathf.Lerp(m_rightArmIK.solver.IKRotationWeight, (l_data.m_rightHand.m_present && !Settings.FingersOnly) ? 1f : 0f, 0.25f);
                    if(Settings.TrackElbows)
                        m_rightArmIK.solver.arm.bendGoalWeight = Mathf.Lerp(m_rightArmIK.solver.arm.bendGoalWeight, (l_data.m_rightHand.m_present && !Settings.FingersOnly) ? 1f : 0f, 0.25f);
                }

                if((m_vrIK != null) && !Settings.FingersOnly)
                {
                    m_leftTargetActive = l_data.m_leftHand.m_present;
                    m_rightTargetActive = l_data.m_rightHand.m_present;
                }
            }
        }

        void LateUpdate()
        {
            if(Settings.Enabled && (m_poseHandler != null))
            {
                LeapParser.LeapData l_data = LeapManager.Instance.GetLatestData();
                if(l_data.m_leftHand.m_present)
                {
                    Quaternion l_turnBack = (m_leftHandOffset.m_source.rotation * m_leftHandOffset.m_offset) * Quaternion.Inverse(m_leftHandOffset.m_target.rotation);
                    foreach(var l_info in m_leftFingerOffsets)
                        l_info.m_target.rotation = l_turnBack * (l_info.m_source.rotation * l_info.m_offset);
                }
                if(l_data.m_rightHand.m_present)
                {
                    Quaternion l_turnBack = (m_rightHandOffset.m_source.rotation * m_rightHandOffset.m_offset) * Quaternion.Inverse(m_rightHandOffset.m_target.rotation);
                    foreach(var l_info in m_rightFingerOffsets)
                        l_info.m_target.rotation = l_turnBack * (l_info.m_source.rotation * l_info.m_offset);
                }

                m_poseHandler.GetHumanPose(ref m_pose);
                if(l_data.m_leftHand.m_present || l_data.m_rightHand.m_present)
                {
                    for(int i = 0; i < 5; i++)
                    {
                        int l_offset = i * 4;

                        ms_lastLeftFingerBones[l_offset] = m_pose.muscles[(int)MuscleIndex.LeftThumb1Stretched + l_offset];
                        ms_lastLeftFingerBones[l_offset + 1] = m_pose.muscles[(int)MuscleIndex.LeftThumb2Stretched + l_offset];
                        ms_lastLeftFingerBones[l_offset + 2] = m_pose.muscles[(int)MuscleIndex.LeftThumb3Stretched + l_offset];
                        ms_lastLeftFingerBones[l_offset + 3] = m_pose.muscles[(int)MuscleIndex.LeftThumbSpread + l_offset];

                        ms_lastRightFingerBones[l_offset] = m_pose.muscles[(int)MuscleIndex.RightThumb1Stretched + l_offset];
                        ms_lastRightFingerBones[l_offset + 1] = m_pose.muscles[(int)MuscleIndex.RightThumb2Stretched + l_offset];
                        ms_lastRightFingerBones[l_offset + 2] = m_pose.muscles[(int)MuscleIndex.RightThumb3Stretched + l_offset];
                        ms_lastRightFingerBones[l_offset + 3] = m_pose.muscles[(int)MuscleIndex.RightThumbSpread + l_offset];
                    }
                }

                if(Settings.MechanimFilter && (m_hips != null))
                {
                    // Yoinked from IKSystem.OnPostSolverUpdateGeneral
                    Vector3 l_pos = m_hips.position;
                    Quaternion l_rot = m_hips.rotation;
                    m_poseHandler.SetHumanPose(ref m_pose);
                    m_hips.SetPositionAndRotation(l_pos, l_rot);
                }
            }
        }

        // Game events
        void OnAvatarClear(CVRAvatar p_avatar)
        {
            try
            {
                m_vrIK = null;
                m_hips = null;
                m_leftArmIK = null;
                m_rightArmIK = null;
                m_leftTargetActive = false;
                m_rightTargetActive = false;

                m_poseHandler?.Dispose();
                m_poseHandler = null;

                m_leftHandTarget.localPosition = Vector3.zero;
                m_leftHandTarget.localRotation = Quaternion.identity;
                m_rightHandTarget.localPosition = Vector3.zero;
                m_rightHandTarget.localRotation = Quaternion.identity;

                m_leftHandOffset.Reset();
                m_rightHandOffset.Reset();

                m_leftFingerOffsets.Clear();
                m_rightFingerOffsets.Clear();
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        void OnAvatarSetup(CVRAvatar p_avatar)
        {
            try
            {
                Animator l_animator = PlayerSetup.Instance.Animator;
                if(l_animator.isHuman)
                {
                    Utils.SetAvatarTPose();

                    m_poseHandler = new HumanPoseHandler(l_animator.avatar, l_animator.transform);
                    m_poseHandler.GetHumanPose(ref m_pose);

                    m_hips = l_animator.GetBoneTransform(HumanBodyBones.Hips);

                    m_leftHandOffset.m_source = l_animator.GetBoneTransform(HumanBodyBones.LeftHand);
                    m_leftHandTarget.localRotation = ms_offsetLeft * (Quaternion.Inverse(l_animator.transform.rotation) * m_leftHandOffset.m_source.rotation);

                    m_rightHandOffset.m_source = l_animator.GetBoneTransform(HumanBodyBones.RightHand);
                    m_rightHandTarget.localRotation = ms_offsetRight * (Quaternion.Inverse(l_animator.transform.rotation) * m_rightHandOffset.m_source.rotation);

                    ParseFingersBones();

                    m_vrIK = l_animator.GetComponent<VRIK>();
                    if(m_vrIK != null)
                    {
                        m_vrIK.onPreSolverUpdate.AddListener(this.OnIKPreSolverUpdate);
                        m_vrIK.onPostSolverUpdate.AddListener(this.OnIKPostSolverUpdate);
                    }
                    else
                        SetupArmIK();
                }
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        void OnAvatarReuse()
        {
            // Old VRIK is destroyed by game
            m_vrIK = PlayerSetup.Instance.AvatarObject.GetComponent<VRIK>();

            if(Utils.IsInVR())
                RemoveArmIK();

            if(m_vrIK != null)
            {
                m_vrIK.onPreSolverUpdate.AddListener(this.OnIKPreSolverUpdate);
                m_vrIK.onPostSolverUpdate.AddListener(this.OnIKPostSolverUpdate);
            }
            else
            {
                Utils.SetAvatarTPose();
                SetupArmIK();
            }
        }

        // VRIK updates
        void OnIKPreSolverUpdate()
        {
            if(m_leftTargetActive)
            {
                m_vrIKInfo.m_leftHandTarget = m_vrIK.solver.leftArm.target;
                m_vrIKInfo.m_armsWeights.x = m_vrIK.solver.leftArm.positionWeight;
                m_vrIKInfo.m_armsWeights.y = m_vrIK.solver.leftArm.rotationWeight;
                m_vrIKInfo.m_leftElbowTarget = m_vrIK.solver.leftArm.bendGoal;
                m_vrIKInfo.m_elbowsWeights.x = m_vrIK.solver.leftArm.bendGoalWeight;

                m_vrIK.solver.leftArm.target = m_leftHandTarget;
                m_vrIK.solver.leftArm.positionWeight = 1f;
                m_vrIK.solver.leftArm.rotationWeight = 1f;
                m_vrIK.solver.leftArm.bendGoal = LeapTracking.Instance.GetLeftElbow();
                m_vrIK.solver.leftArm.bendGoalWeight = (Settings.TrackElbows ? 1f : 0f);
            }
            if(m_rightTargetActive)
            {
                m_vrIKInfo.m_rightHandTarget = m_vrIK.solver.rightArm.target;
                m_vrIKInfo.m_armsWeights.z = m_vrIK.solver.rightArm.positionWeight;
                m_vrIKInfo.m_armsWeights.w = m_vrIK.solver.rightArm.rotationWeight;
                m_vrIKInfo.m_rightElbowTarget = m_vrIK.solver.rightArm.bendGoal;
                m_vrIKInfo.m_elbowsWeights.y = m_vrIK.solver.rightArm.bendGoalWeight;

                m_vrIK.solver.rightArm.target = m_rightHandTarget;
                m_vrIK.solver.rightArm.positionWeight = 1f;
                m_vrIK.solver.rightArm.rotationWeight = 1f;
                m_vrIK.solver.rightArm.bendGoal = LeapTracking.Instance.GetRightElbow();
                m_vrIK.solver.rightArm.bendGoalWeight = (Settings.TrackElbows ? 1f : 0f);
            }
        }
        void OnIKPostSolverUpdate()
        {
            if(m_leftTargetActive)
            {
                m_vrIK.solver.leftArm.target = m_vrIKInfo.m_leftHandTarget;
                m_vrIK.solver.leftArm.positionWeight = m_vrIKInfo.m_armsWeights.x;
                m_vrIK.solver.leftArm.rotationWeight = m_vrIKInfo.m_armsWeights.y;
                m_vrIK.solver.leftArm.bendGoal = m_vrIKInfo.m_leftElbowTarget;
                m_vrIK.solver.leftArm.bendGoalWeight = m_vrIKInfo.m_elbowsWeights.x;
            }
            if(m_rightTargetActive)
            {
                m_vrIK.solver.rightArm.target = m_vrIKInfo.m_rightHandTarget;
                m_vrIK.solver.rightArm.positionWeight = m_vrIKInfo.m_armsWeights.z;
                m_vrIK.solver.rightArm.rotationWeight = m_vrIKInfo.m_armsWeights.w;
                m_vrIK.solver.rightArm.bendGoal = m_vrIKInfo.m_rightElbowTarget;
                m_vrIK.solver.rightArm.bendGoalWeight = m_vrIKInfo.m_elbowsWeights.y;
            }
        }

        // Settings
        void OnEnabledOrFingersOnlyChanged(bool p_state)
        {
            RefreshArmIK();
            ResetTargetsStates();
        }

        void OnTrackElbowsChanged(bool p_state)
        {
            if((m_leftArmIK != null) && (m_rightArmIK != null))
            {
                m_leftArmIK.solver.arm.bendGoalWeight = (p_state ? 1f : 0f);
                m_rightArmIK.solver.arm.bendGoalWeight = (p_state ? 1f : 0f);
            }

            ResetTargetsStates();
        }

        // Arbitrary
        void ResetTargetsStates()
        {
            m_leftTargetActive = false;
            m_rightTargetActive = false;
        }

        void SetupArmIK()
        {
            Animator l_animator = PlayerSetup.Instance.Animator;
            Transform l_chest = l_animator.GetBoneTransform(HumanBodyBones.UpperChest);
            if(l_chest == null)
                l_chest = l_animator.GetBoneTransform(HumanBodyBones.Chest);
            if(l_chest == null)
                l_chest = l_animator.GetBoneTransform(HumanBodyBones.Spine);

            m_leftArmIK = l_animator.gameObject.AddComponent<ArmIK>();
            m_leftArmIK.solver.isLeft = true;
            m_leftArmIK.solver.SetChain(
                l_chest,
                l_animator.GetBoneTransform(HumanBodyBones.LeftShoulder),
                l_animator.GetBoneTransform(HumanBodyBones.LeftUpperArm),
                l_animator.GetBoneTransform(HumanBodyBones.LeftLowerArm),
                l_animator.GetBoneTransform(HumanBodyBones.LeftHand),
                l_animator.transform
            );
            m_leftArmIK.solver.arm.target = m_leftHandTarget;
            m_leftArmIK.solver.arm.bendGoal = LeapTracking.Instance.GetLeftElbow();
            m_leftArmIK.solver.arm.bendGoalWeight = (Settings.TrackElbows ? 1f : 0f);
            m_leftArmIK.enabled = (Settings.Enabled && !Settings.FingersOnly);

            m_rightArmIK = l_animator.gameObject.AddComponent<ArmIK>();
            m_rightArmIK.solver.isLeft = false;
            m_rightArmIK.solver.SetChain(
                l_chest,
                l_animator.GetBoneTransform(HumanBodyBones.RightShoulder),
                l_animator.GetBoneTransform(HumanBodyBones.RightUpperArm),
                l_animator.GetBoneTransform(HumanBodyBones.RightLowerArm),
                l_animator.GetBoneTransform(HumanBodyBones.RightHand),
                l_animator.transform
            );
            m_rightArmIK.solver.arm.target = m_rightHandTarget;
            m_rightArmIK.solver.arm.bendGoal = LeapTracking.Instance.GetRightElbow();
            m_rightArmIK.solver.arm.bendGoalWeight = (Settings.TrackElbows ? 1f : 0f);
            m_rightArmIK.enabled = (Settings.Enabled && !Settings.FingersOnly);
        }

        void RemoveArmIK()
        {
            if(m_leftArmIK != null)
                Object.Destroy(m_leftArmIK);
            m_leftArmIK = null;

            if(m_rightArmIK != null)
                Object.Destroy(m_rightArmIK);
            m_rightArmIK = null;
        }

        void RefreshArmIK()
        {
            if((m_leftArmIK != null) && (m_rightArmIK != null))
            {
                m_leftArmIK.enabled = (Settings.Enabled && !Settings.FingersOnly);
                m_rightArmIK.enabled = (Settings.Enabled && !Settings.FingersOnly);
            }
        }

        void ParseFingersBones()
        {
            LeapTracking.Instance.Rebind(PlayerSetup.Instance.transform.rotation);

            // Align rotations of leap fingers to avatar fingers
            Animator l_animator = PlayerSetup.Instance.Animator;
            LeapHand l_leapLeft = LeapTracking.Instance.GetLeftHand();
            LeapHand l_leapRight = LeapTracking.Instance.GetRightHand();
            // Try to "fix" rotations, slightly inaccurate after 0YX plane rotation
            foreach(var l_tuple in ms_fingersChains)
            {
                ReorientateTowards(
                    PlayerSetup.Instance.transform,
                    l_animator.GetBoneTransform(l_tuple.Item1),
                    (l_tuple.Item2 != HumanBodyBones.LastBone) ? l_animator.GetBoneTransform(l_tuple.Item2) : null,
                    l_tuple.Item3 ? l_leapLeft.GetLinkedBone(l_tuple.Item1) : l_leapRight.GetLinkedBone(l_tuple.Item1),
                    l_tuple.Item3 ? l_leapLeft.GetLinkedBone(l_tuple.Item2) : l_leapRight.GetLinkedBone(l_tuple.Item2),
                    PlaneType.OXZ
                );
                ReorientateTowards(
                    PlayerSetup.Instance.transform,
                    l_animator.GetBoneTransform(l_tuple.Item1),
                    (l_tuple.Item2 != HumanBodyBones.LastBone) ? l_animator.GetBoneTransform(l_tuple.Item2) : null,
                    l_tuple.Item3 ? l_leapLeft.GetLinkedBone(l_tuple.Item1) : l_leapRight.GetLinkedBone(l_tuple.Item1),
                    l_tuple.Item3 ? l_leapLeft.GetLinkedBone(l_tuple.Item2) : l_leapRight.GetLinkedBone(l_tuple.Item2),
                    PlaneType.OYX
                );
            }

            // Bind
            m_leftHandOffset.m_target = l_leapLeft.GetLinkedBone(HumanBodyBones.LeftHand);
            if((m_leftHandOffset.m_source != null) && (m_leftHandOffset.m_target != null))
                m_leftHandOffset.m_offset = Quaternion.Inverse(m_leftHandOffset.m_source.rotation) * m_leftHandOffset.m_target.rotation;

            m_rightHandOffset.m_target = l_leapRight.GetLinkedBone(HumanBodyBones.RightHand);
            if((m_rightHandOffset.m_source != null) && (m_rightHandOffset.m_target != null))
                m_rightHandOffset.m_offset = Quaternion.Inverse(m_rightHandOffset.m_source.rotation) * m_rightHandOffset.m_target.rotation;

            foreach(var l_link in ms_fingers)
            {
                Transform l_transform = l_animator.GetBoneTransform(l_link.Item1);
                if(l_transform != null)
                {
                    RotationOffset l_offset = new RotationOffset();
                    l_offset.m_target = l_transform;
                    l_offset.m_source = (l_link.Item2 ? l_leapLeft.GetLinkedBone(l_link.Item1) : l_leapRight.GetLinkedBone(l_link.Item1));
                    l_offset.m_offset = Quaternion.Inverse(l_offset.m_source.rotation) * l_offset.m_target.rotation;

                    if(l_link.Item2)
                        m_leftFingerOffsets.Add(l_offset);
                    else
                        m_rightFingerOffsets.Add(l_offset);
                }
            }
        }

        static void ReorientateTowards(Transform root, Transform p_source, Transform p_sourceEnd, Transform p_target, Transform p_targetEnd, PlaneType p_plane)
        {
            if((root != null) && (p_target != null) && (p_source != null))
            {
                Quaternion l_rootInv = Quaternion.Inverse(root.rotation);
                Vector3 l_targetDir = l_rootInv * (((p_targetEnd != null) ? p_targetEnd.position : GuessEnd(p_target)) - p_target.position);
                Vector3 l_sourceDir = l_rootInv * (((p_sourceEnd != null) ? p_sourceEnd.position : GuessEnd(p_source)) - p_source.position);
                switch(p_plane)
                {
                    case PlaneType.OXZ:
                        l_targetDir.y = 0f;
                        l_sourceDir.y = 0f;
                        break;
                    case PlaneType.OYX:
                        l_targetDir.z = 0f;
                        l_sourceDir.z = 0f;
                        break;
                }
                l_targetDir = Vector3.Normalize(l_targetDir);
                l_sourceDir = Vector3.Normalize(l_sourceDir);

                Quaternion l_targetRot = Quaternion.identity;
                Quaternion l_sourceRot = Quaternion.identity;
                switch(p_plane)
                {
                    case PlaneType.OXZ:
                        l_targetRot = Quaternion.LookRotation(l_targetDir, Vector3.up);
                        l_sourceRot = Quaternion.LookRotation(l_sourceDir, Vector3.up);
                        break;
                    case PlaneType.OYX:
                        l_targetRot = Quaternion.LookRotation(l_targetDir, Vector3.forward);
                        l_sourceRot = Quaternion.LookRotation(l_sourceDir, Vector3.forward);
                        break;
                }

                Quaternion l_diff = Quaternion.Inverse(l_targetRot) * l_sourceRot;
                if(p_plane == PlaneType.OYX)
                    l_diff = Quaternion.Euler(0f, 0f, l_diff.eulerAngles.y);

                Quaternion l_adjusted = l_diff * (l_rootInv * p_target.rotation);
                p_target.rotation = root.rotation * l_adjusted;
            }
        }

        static Vector3 GuessEnd(Transform p_target)
        {
            Vector3 l_result = p_target.position;
            if(p_target.parent != null)
            {
                float l_dot = -1f;
                Vector3 l_axisDir = p_target.position - p_target.parent.position;
                foreach(Vector3 l_dir in ms_directions)
                {
                    Vector3 l_rotDir = p_target.rotation * l_dir;
                    float l_stepDot = Vector3.Dot(l_rotDir, l_axisDir);
                    if(l_stepDot >= l_dot)
                    {
                        l_dot = l_stepDot;
                        l_result = p_target.position + l_rotDir;
                    }
                }
            }
            return l_result;
        }
    }
}

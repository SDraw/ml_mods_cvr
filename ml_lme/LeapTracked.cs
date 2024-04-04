using ABI_RC.Core.Player;
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
        static readonly (HumanBodyBones, HumanBodyBones, bool)[] ms_rotationFixChains =
        {
            (HumanBodyBones.LeftThumbProximal,HumanBodyBones.LeftThumbIntermediate,true), (HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal,true),
            (HumanBodyBones.LeftIndexProximal,HumanBodyBones.LeftIndexIntermediate,true), (HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal,true),
            (HumanBodyBones.LeftMiddleProximal,HumanBodyBones.LeftMiddleIntermediate,true), (HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal,true),
            (HumanBodyBones.LeftRingProximal,HumanBodyBones.LeftRingIntermediate,true), (HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal,true),
            (HumanBodyBones.LeftLittleProximal,HumanBodyBones.LeftLittleIntermediate,true), (HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal,true),
            (HumanBodyBones.RightThumbProximal,HumanBodyBones.RightThumbIntermediate,false), (HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbDistal,false),
            (HumanBodyBones.RightIndexProximal,HumanBodyBones.RightIndexIntermediate,false), (HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal,false),
            (HumanBodyBones.RightMiddleProximal,HumanBodyBones.RightMiddleIntermediate,false), (HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal,false),
            (HumanBodyBones.RightRingProximal,HumanBodyBones.RightRingIntermediate,false), (HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal,false),
            (HumanBodyBones.RightLittleProximal,HumanBodyBones.RightLittleIntermediate,false), (HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal,false)
        };

        public static readonly float[] ms_lastLeftFingerBones = new float[20];
        public static readonly float[] ms_lastRightFingerBones = new float[20];

        bool m_inVR = false;
        VRIK m_vrIK = null;
        Transform m_hips = null;

        bool m_enabled = true;
        bool m_fingersOnly = false;
        bool m_trackElbows = true;

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
            m_inVR = Utils.IsInVR();

            m_leftHandTarget = new GameObject("RotationTarget").transform;
            m_leftHandTarget.parent = LeapTracking.Instance.GetLeftHand().GetRoot();
            m_leftHandTarget.localPosition = Vector3.zero;
            m_leftHandTarget.localRotation = Quaternion.identity;

            m_rightHandTarget = new GameObject("RotationTarget").transform;
            m_rightHandTarget.parent = LeapTracking.Instance.GetRightHand().GetRoot();
            m_rightHandTarget.localPosition = Vector3.zero;
            m_rightHandTarget.localRotation = Quaternion.identity;

            Settings.EnabledChange += this.OnEnabledChange;
            Settings.FingersOnlyChange += this.OnFingersOnlyChange;
            Settings.TrackElbowsChange += this.OnTrackElbowsChange;

            OnEnabledChange(Settings.Enabled);
            OnFingersOnlyChange(Settings.FingersOnly);
            OnTrackElbowsChange(Settings.TrackElbows);
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

            Settings.EnabledChange -= this.OnEnabledChange;
            Settings.FingersOnlyChange -= this.OnFingersOnlyChange;
            Settings.TrackElbowsChange -= this.OnTrackElbowsChange;
        }

        void Update()
        {
            if(m_enabled)
            {
                LeapParser.LeapData l_data = LeapManager.Instance.GetLatestData();

                if((m_leftArmIK != null) && (m_rightArmIK != null))
                {
                    m_leftArmIK.solver.IKPositionWeight = Mathf.Lerp(m_leftArmIK.solver.IKPositionWeight, (l_data.m_leftHand.m_present && !m_fingersOnly) ? 1f : 0f, 0.25f);
                    m_leftArmIK.solver.IKRotationWeight = Mathf.Lerp(m_leftArmIK.solver.IKRotationWeight, (l_data.m_leftHand.m_present && !m_fingersOnly) ? 1f : 0f, 0.25f);
                    if(m_trackElbows)
                        m_leftArmIK.solver.arm.bendGoalWeight = Mathf.Lerp(m_leftArmIK.solver.arm.bendGoalWeight, (l_data.m_leftHand.m_present && !m_fingersOnly) ? 1f : 0f, 0.25f);

                    m_rightArmIK.solver.IKPositionWeight = Mathf.Lerp(m_rightArmIK.solver.IKPositionWeight, (l_data.m_rightHand.m_present && !m_fingersOnly) ? 1f : 0f, 0.25f);
                    m_rightArmIK.solver.IKRotationWeight = Mathf.Lerp(m_rightArmIK.solver.IKRotationWeight, (l_data.m_rightHand.m_present && !m_fingersOnly) ? 1f : 0f, 0.25f);
                    if(m_trackElbows)
                        m_rightArmIK.solver.arm.bendGoalWeight = Mathf.Lerp(m_rightArmIK.solver.arm.bendGoalWeight, (l_data.m_rightHand.m_present && !m_fingersOnly) ? 1f : 0f, 0.25f);
                }

                if((m_vrIK != null) && !m_fingersOnly)
                {
                    m_leftTargetActive = l_data.m_leftHand.m_present;
                    m_rightTargetActive = l_data.m_rightHand.m_present;
                }
            }
        }

        void LateUpdate()
        {
            if(m_enabled && (m_poseHandler != null))
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
        internal void OnAvatarClear()
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

        internal void OnAvatarSetup()
        {
            m_inVR = Utils.IsInVR();

            if(PlayerSetup.Instance._animator.isHuman)
            {
                Utils.SetAvatarTPose();

                m_poseHandler = new HumanPoseHandler(PlayerSetup.Instance._animator.avatar, PlayerSetup.Instance._animator.transform);
                m_poseHandler.GetHumanPose(ref m_pose);

                m_hips = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Hips);

                m_leftHandOffset.m_source = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftHand);
                m_leftHandTarget.localRotation = ms_offsetLeft * (Quaternion.Inverse(PlayerSetup.Instance._avatar.transform.rotation) * m_leftHandOffset.m_source.rotation);

                m_rightHandOffset.m_source = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand);
                m_rightHandTarget.localRotation = ms_offsetRight * (Quaternion.Inverse(PlayerSetup.Instance._avatar.transform.rotation) * m_rightHandOffset.m_source.rotation);

                ParseFingersBones();

                m_vrIK = PlayerSetup.Instance._animator.GetComponent<VRIK>();
                if(m_vrIK != null)
                {
                    m_vrIK.onPreSolverUpdate.AddListener(this.OnIKPreUpdate);
                    m_vrIK.onPostSolverUpdate.AddListener(this.OnIKPostUpdate);
                }
                else
                    SetupArmIK();
            }
        }

        internal void OnAvatarReinitialize()
        {
            // Old VRIK is destroyed by game
            m_inVR = Utils.IsInVR();
            m_vrIK = PlayerSetup.Instance._animator.GetComponent<VRIK>();

            if(m_inVR)
                RemoveArmIK();

            if(m_vrIK != null)
            {
                m_vrIK.onPreSolverUpdate.AddListener(this.OnIKPreUpdate);
                m_vrIK.onPostSolverUpdate.AddListener(this.OnIKPostUpdate);
            }
            else
            {
                Utils.SetAvatarTPose();
                SetupArmIK();
            }
        }

        // VRIK updates
        void OnIKPreUpdate()
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
                m_vrIK.solver.leftArm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
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
                m_vrIK.solver.rightArm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
            }
        }
        void OnIKPostUpdate()
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
        void OnEnabledChange(bool p_state)
        {
            m_enabled = p_state;

            RefreshArmIK();
            ResetTargetsStates();
        }

        void OnFingersOnlyChange(bool p_state)
        {
            m_fingersOnly = p_state;

            RefreshArmIK();
            ResetTargetsStates();
        }

        void OnTrackElbowsChange(bool p_state)
        {
            m_trackElbows = p_state;

            if((m_leftArmIK != null) && (m_rightArmIK != null))
            {
                m_leftArmIK.solver.arm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
                m_rightArmIK.solver.arm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
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
            Transform l_chest = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.UpperChest);
            if(l_chest == null)
                l_chest = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Chest);
            if(l_chest == null)
                l_chest = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Spine);

            m_leftArmIK = PlayerSetup.Instance._avatar.AddComponent<ArmIK>();
            m_leftArmIK.solver.isLeft = true;
            m_leftArmIK.solver.SetChain(
                l_chest,
                PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftShoulder),
                PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftUpperArm),
                PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftLowerArm),
                PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftHand),
                PlayerSetup.Instance._animator.transform
            );
            m_leftArmIK.solver.arm.target = m_leftHandTarget;
            m_leftArmIK.solver.arm.bendGoal = LeapTracking.Instance.GetLeftElbow();
            m_leftArmIK.solver.arm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
            m_leftArmIK.enabled = (m_enabled && !m_fingersOnly);

            m_rightArmIK = PlayerSetup.Instance._avatar.AddComponent<ArmIK>();
            m_rightArmIK.solver.isLeft = false;
            m_rightArmIK.solver.SetChain(
                l_chest,
                PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightShoulder),
                PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightUpperArm),
                PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightLowerArm),
                PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand),
                PlayerSetup.Instance._animator.transform
            );
            m_rightArmIK.solver.arm.target = m_rightHandTarget;
            m_rightArmIK.solver.arm.bendGoal = LeapTracking.Instance.GetRightElbow();
            m_rightArmIK.solver.arm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
            m_rightArmIK.enabled = (m_enabled && !m_fingersOnly);
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
                m_leftArmIK.enabled = (m_enabled && !m_fingersOnly);
                m_rightArmIK.enabled = (m_enabled && !m_fingersOnly);
            }
        }

        void ParseFingersBones()
        {
            LeapTracking.Instance.Rebind(PlayerSetup.Instance.transform.rotation);

            // Try to "fix" rotations, slightly inaccurate after 0YX plane rotation
            foreach(var l_tuple in ms_rotationFixChains)
            {
                ReorientateTowards(
                    PlayerSetup.Instance._animator.GetBoneTransform(l_tuple.Item1),
                    PlayerSetup.Instance._animator.GetBoneTransform(l_tuple.Item2),
                    l_tuple.Item3 ? LeapTracking.Instance.GetLeftHand().GetBone(l_tuple.Item1) : LeapTracking.Instance.GetRightHand().GetBone(l_tuple.Item1),
                    l_tuple.Item3 ? LeapTracking.Instance.GetLeftHand().GetBone(l_tuple.Item2) : LeapTracking.Instance.GetRightHand().GetBone(l_tuple.Item2),
                    PlaneType.OXZ
                );
                ReorientateTowards(
                    PlayerSetup.Instance._animator.GetBoneTransform(l_tuple.Item1),
                    PlayerSetup.Instance._animator.GetBoneTransform(l_tuple.Item2),
                    l_tuple.Item3 ? LeapTracking.Instance.GetLeftHand().GetBone(l_tuple.Item1) : LeapTracking.Instance.GetRightHand().GetBone(l_tuple.Item1),
                    l_tuple.Item3 ? LeapTracking.Instance.GetLeftHand().GetBone(l_tuple.Item2) : LeapTracking.Instance.GetRightHand().GetBone(l_tuple.Item2),
                    PlaneType.OYX
                );
            }

            // Bind
            m_leftHandOffset.m_target = LeapTracking.Instance.GetLeftHand().GetBone(HumanBodyBones.LeftHand);
            if((m_leftHandOffset.m_source != null) && (m_leftHandOffset.m_target != null))
                m_leftHandOffset.m_offset = Quaternion.Inverse(m_leftHandOffset.m_source.rotation) * m_leftHandOffset.m_target.rotation;

            m_rightHandOffset.m_target = LeapTracking.Instance.GetRightHand().GetBone(HumanBodyBones.RightHand);
            if((m_rightHandOffset.m_source != null) && (m_rightHandOffset.m_target != null))
                m_rightHandOffset.m_offset = Quaternion.Inverse(m_rightHandOffset.m_source.rotation) * m_rightHandOffset.m_target.rotation;

            foreach(var l_link in ms_fingers)
            {
                Transform l_transform = PlayerSetup.Instance._animator.GetBoneTransform(l_link.Item1);
                if(l_transform != null)
                {
                    RotationOffset l_offset = new RotationOffset();
                    l_offset.m_target = l_transform;
                    l_offset.m_source = (l_link.Item2 ? LeapTracking.Instance.GetLeftHand().GetBone(l_link.Item1) : LeapTracking.Instance.GetRightHand().GetBone(l_link.Item1));
                    l_offset.m_offset = Quaternion.Inverse(l_offset.m_source.rotation) * l_offset.m_target.rotation;

                    if(l_link.Item2)
                        m_leftFingerOffsets.Add(l_offset);
                    else
                        m_rightFingerOffsets.Add(l_offset);
                }
            }
        }

        void ReorientateTowards(Transform p_target, Transform p_targetEnd, Transform p_source, Transform p_sourceEnd, PlaneType p_plane)
        {
            if((p_target != null) && (p_targetEnd != null) && (p_source != null) && (p_sourceEnd != null))
            {
                Quaternion l_playerInv = Quaternion.Inverse(PlayerSetup.Instance.transform.rotation);
                Vector3 l_targetDir = l_playerInv * (p_targetEnd.position - p_target.position);
                Vector3 l_sourceDir = l_playerInv * (p_sourceEnd.position - p_source.position);
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

                Quaternion l_adjusted = l_diff * (l_playerInv * p_target.rotation);
                p_target.rotation = PlayerSetup.Instance.transform.rotation * l_adjusted;
            }
        }
    }
}

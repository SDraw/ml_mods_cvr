using ABI_RC.Core.Player;
using ABI_RC.Systems.IK;
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ml_lme
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(999999)]
    class LeapTracked : MonoBehaviour
    {
        struct IKInfo
        {
            public Vector4 m_armsWeights;
            public Vector2 m_elbowsWeights;
            public Transform m_leftHandTarget;
            public Transform m_rightHandTarget;
            public Transform m_leftElbowTarget;
            public Transform m_rightElbowTarget;
        }

        struct FingerBoneInfo
        {
            public LeapHand.FingerBone m_bone;
            public Transform m_targetBone;
            public Transform m_sourceBone;
            public Quaternion m_offset;
        }

        static readonly Quaternion ms_offsetLeft = Quaternion.Euler(0f, 90f, 0f);
        static readonly Quaternion ms_offsetRight = Quaternion.Euler(0f, 270f, 0f);

        static readonly (HumanBodyBones, LeapHand.FingerBone, bool)[] ms_fingerBonesLinks =
        {
            (HumanBodyBones.LeftThumbProximal, LeapHand.FingerBone.ThumbProximal, true),
            (HumanBodyBones.LeftThumbIntermediate, LeapHand.FingerBone.ThumbIntermediate, true),
            (HumanBodyBones.LeftThumbDistal, LeapHand.FingerBone.ThumbDistal, true),
            (HumanBodyBones.LeftIndexProximal, LeapHand.FingerBone.IndexProximal, true),
            (HumanBodyBones.LeftIndexIntermediate, LeapHand.FingerBone.IndexIntermediate, true),
            (HumanBodyBones.LeftIndexDistal, LeapHand.FingerBone.IndexDistal, true),
            (HumanBodyBones.LeftMiddleProximal, LeapHand.FingerBone.MiddleProximal, true),
            (HumanBodyBones.LeftMiddleIntermediate, LeapHand.FingerBone.MiddleIntermediate, true),
            (HumanBodyBones.LeftMiddleDistal, LeapHand.FingerBone.MiddleDistal, true),
            (HumanBodyBones.LeftRingProximal, LeapHand.FingerBone.RingProximal, true),
            (HumanBodyBones.LeftRingIntermediate, LeapHand.FingerBone.RingIntermediate, true),
            (HumanBodyBones.LeftRingDistal, LeapHand.FingerBone.RingDistal, true),
            (HumanBodyBones.LeftLittleProximal, LeapHand.FingerBone.PinkyProximal, true),
            (HumanBodyBones.LeftLittleIntermediate, LeapHand.FingerBone.PinkyIntermediate, true),
            (HumanBodyBones.LeftLittleDistal, LeapHand.FingerBone.PinkyDistal, true),

            (HumanBodyBones.RightThumbProximal, LeapHand.FingerBone.ThumbProximal, false),
            (HumanBodyBones.RightThumbIntermediate, LeapHand.FingerBone.ThumbIntermediate, false),
            (HumanBodyBones.RightThumbDistal, LeapHand.FingerBone.ThumbDistal, false),
            (HumanBodyBones.RightIndexProximal, LeapHand.FingerBone.IndexProximal, false),
            (HumanBodyBones.RightIndexIntermediate, LeapHand.FingerBone.IndexIntermediate, false),
            (HumanBodyBones.RightIndexDistal, LeapHand.FingerBone.IndexDistal, false),
            (HumanBodyBones.RightMiddleProximal, LeapHand.FingerBone.MiddleProximal, false),
            (HumanBodyBones.RightMiddleIntermediate, LeapHand.FingerBone.MiddleIntermediate, false),
            (HumanBodyBones.RightMiddleDistal, LeapHand.FingerBone.MiddleDistal, false),
            (HumanBodyBones.RightRingProximal, LeapHand.FingerBone.RingProximal, false),
            (HumanBodyBones.RightRingIntermediate, LeapHand.FingerBone.RingIntermediate, false),
            (HumanBodyBones.RightRingDistal, LeapHand.FingerBone.RingDistal, false),
            (HumanBodyBones.RightLittleProximal, LeapHand.FingerBone.PinkyProximal, false),
            (HumanBodyBones.RightLittleIntermediate, LeapHand.FingerBone.PinkyIntermediate, false),
            (HumanBodyBones.RightLittleDistal, LeapHand.FingerBone.PinkyDistal, false),
        };

        public static readonly float[] ms_lastLeftFingerBones = new float[20];
        public static readonly float[] ms_lastRightFingerBones = new float[20];

        bool m_inVR = false;
        VRIK m_vrIK = null;

        bool m_enabled = true;
        bool m_fingersOnly = false;
        bool m_trackElbows = true;

        Transform m_leftHand = null;
        Transform m_rightHand = null;
        IKInfo m_vrIKInfo;
        ArmIK m_leftArmIK = null;
        ArmIK m_rightArmIK = null;
        HumanPoseHandler m_poseHandler = null;
        HumanPose m_pose;
        Transform m_leftHandTarget = null;
        Transform m_rightHandTarget = null;
        bool m_leftTargetActive = false; // VRIK only
        bool m_rightTargetActive = false; // VRIK only

        readonly List<FingerBoneInfo> m_leftFingerBones = null;
        readonly List<FingerBoneInfo> m_rightFingerBones = null;

        Quaternion m_leftWristOffset;
        Quaternion m_rightWristOffset;

        internal LeapTracked()
        {
            m_leftFingerBones = new List<FingerBoneInfo>();
            m_rightFingerBones = new List<FingerBoneInfo>();
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
                    Transform l_leapWrist = LeapTracking.Instance.GetLeftHand().GetWrist();
                    Quaternion l_turnBack = (m_leftHand.rotation * m_leftWristOffset) * Quaternion.Inverse(l_leapWrist.rotation);
                    foreach(var l_info in m_leftFingerBones)
                        l_info.m_targetBone.rotation = l_turnBack * (l_info.m_sourceBone.rotation * l_info.m_offset);
                }
                if(l_data.m_rightHand.m_present)
                {
                    Transform l_leapWrist = LeapTracking.Instance.GetRightHand().GetWrist();
                    Quaternion l_turnBack = (m_rightHand.rotation * m_rightWristOffset) * Quaternion.Inverse(l_leapWrist.rotation);
                    foreach(var l_info in m_rightFingerBones)
                        l_info.m_targetBone.rotation = l_turnBack * (l_info.m_sourceBone.rotation * l_info.m_offset);
                }

                m_poseHandler.GetHumanPose(ref m_pose);
                if(l_data.m_leftHand.m_present)
                {
                    for(int i = 0; i < 5; i++)
                    {
                        int l_offset = i * 4;
                        ms_lastLeftFingerBones[l_offset] = m_pose.muscles[(int)MuscleIndex.LeftThumb1Stretched + l_offset];
                        ms_lastLeftFingerBones[l_offset + 1] = m_pose.muscles[(int)MuscleIndex.LeftThumb2Stretched + l_offset];
                        ms_lastLeftFingerBones[l_offset + 2] = m_pose.muscles[(int)MuscleIndex.LeftThumb3Stretched + l_offset];
                        ms_lastLeftFingerBones[l_offset + 3] = m_pose.muscles[(int)MuscleIndex.LeftThumbSpread + l_offset];
                    }
                }
                if(l_data.m_rightHand.m_present)
                {
                    for(int i = 0; i < 5; i++)
                    {
                        int l_offset = i * 4;
                        ms_lastRightFingerBones[l_offset] = m_pose.muscles[(int)MuscleIndex.RightThumb1Stretched + l_offset];
                        ms_lastRightFingerBones[l_offset + 1] = m_pose.muscles[(int)MuscleIndex.RightThumb2Stretched + l_offset];
                        ms_lastRightFingerBones[l_offset + 2] = m_pose.muscles[(int)MuscleIndex.RightThumb3Stretched + l_offset];
                        ms_lastRightFingerBones[l_offset + 3] = m_pose.muscles[(int)MuscleIndex.RightThumbSpread + l_offset];
                    }
                }
            }
        }

        // Game events
        internal void OnAvatarClear()
        {
            m_vrIK = null;
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

            m_leftFingerBones.Clear();
            m_rightFingerBones.Clear();

            m_leftHand = null;
            m_rightHand = null;
            m_leftWristOffset = Quaternion.identity;
            m_rightWristOffset = Quaternion.identity;
        }

        internal void OnAvatarSetup()
        {
            m_inVR = Utils.IsInVR();
            m_vrIK = PlayerSetup.Instance._animator.GetComponent<VRIK>();

            if(PlayerSetup.Instance._animator.isHuman)
            {
                m_poseHandler = new HumanPoseHandler(PlayerSetup.Instance._animator.avatar, PlayerSetup.Instance._animator.transform);
                m_poseHandler.GetHumanPose(ref m_pose);

                if(m_inVR)
                {
                    PlayerSetup.Instance._avatar.transform.localPosition = Vector3.zero;
                    PlayerSetup.Instance._avatar.transform.localRotation = Quaternion.identity;
                }
                else
                    PoseHelper.ForceTPose(PlayerSetup.Instance._animator);

                m_leftHand = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftHand);
                m_leftHandTarget.localRotation = ms_offsetLeft * (Quaternion.Inverse(PlayerSetup.Instance._avatar.transform.rotation) * m_leftHand.rotation);

                m_rightHand = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand);
                m_rightHandTarget.localRotation = ms_offsetRight * (Quaternion.Inverse(PlayerSetup.Instance._avatar.transform.rotation) * m_rightHand.rotation);

                ParseFingersBones();

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
                PoseHelper.ForceTPose(PlayerSetup.Instance._animator);
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
            LeapTracking.Instance.GetLeftHand().Reset();
            LeapTracking.Instance.GetLeftHand().GetWrist().rotation = PlayerSetup.Instance.transform.rotation * ms_offsetRight; // Weird, but that's how it works
            m_leftWristOffset = Quaternion.Inverse(m_leftHand.rotation) * LeapTracking.Instance.GetLeftHand().GetWrist().rotation;

            LeapTracking.Instance.GetRightHand().Reset();
            LeapTracking.Instance.GetRightHand().GetWrist().rotation = PlayerSetup.Instance.transform.rotation * ms_offsetLeft; // Weird, but that's how it works
            m_rightWristOffset = Quaternion.Inverse(m_rightHand.rotation) * LeapTracking.Instance.GetRightHand().GetWrist().rotation;

            foreach(var l_link in ms_fingerBonesLinks)
            {
                Transform l_transform = PlayerSetup.Instance._animator.GetBoneTransform(l_link.Item1);
                if(l_transform != null)
                {
                    FingerBoneInfo l_info = new FingerBoneInfo();
                    l_info.m_bone = l_link.Item2;
                    l_info.m_targetBone = l_transform;
                    l_info.m_sourceBone = (l_link.Item3 ? LeapTracking.Instance.GetLeftHand().GetFingersBone(l_link.Item2) : LeapTracking.Instance.GetRightHand().GetFingersBone(l_link.Item2));
                    l_info.m_offset = Quaternion.Inverse(l_info.m_sourceBone.rotation) * l_info.m_targetBone.rotation;

                    if(l_link.Item3)
                        m_leftFingerBones.Add(l_info);
                    else
                        m_rightFingerBones.Add(l_info);
                }
            }
        }
    }
}

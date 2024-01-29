﻿using ABI_RC.Core.Player;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.VRModeSwitch;
using RootMotion.FinalIK;
using System.Reflection;
using UnityEngine;

namespace ml_lme
{
    [DisallowMultipleComponent]
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

        static readonly Quaternion ms_offsetLeft = Quaternion.Euler(0f, 90f, 0f);
        static readonly Quaternion ms_offsetRight = Quaternion.Euler(0f, 270f, 0f);

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

        // Unity events
        void Start()
        {
            m_inVR = Utils.IsInVR();

            m_leftHandTarget = new GameObject("RotationTarget").transform;
            m_leftHandTarget.parent = LeapTracking.Instance.GetLeftHand();
            m_leftHandTarget.localPosition = Vector3.zero;
            m_leftHandTarget.localRotation = Quaternion.identity;

            m_rightHandTarget = new GameObject("RotationTarget").transform;
            m_rightHandTarget.parent = LeapTracking.Instance.GetRightHand();
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
            if(m_leftArmIK != null)
                Destroy(m_leftArmIK);
            m_leftArmIK = null;

            if(m_rightArmIK != null)
                Destroy(m_rightArmIK);
            m_rightArmIK = null;

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
            if(m_enabled && (m_vrIK == null) && (m_poseHandler != null))
            {
                LeapParser.LeapData l_data = LeapManager.Instance.GetLatestData();

                Vector3 l_hipsLocalPos = m_hips.localPosition;
                Quaternion l_hipsLocalRot = m_hips.localRotation;

                m_poseHandler.GetHumanPose(ref m_pose);
                UpdateFingers(l_data);
                m_poseHandler.SetHumanPose(ref m_pose);

                m_hips.localPosition = l_hipsLocalPos;
                m_hips.localRotation = l_hipsLocalRot;
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
        }

        internal void OnAvatarSetup()
        {
            m_inVR = Utils.IsInVR();
            m_vrIK = PlayerSetup.Instance._animator.GetComponent<VRIK>();

            if(PlayerSetup.Instance._animator.isHuman)
            {
                Vector3 l_hipsPos = Vector3.zero;
                m_hips = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Hips);
                if(m_hips != null)
                    l_hipsPos = m_hips.localPosition;

                if(!m_inVR)
                {
                    // Force desktop avatar into T-Pose
                    m_poseHandler = new HumanPoseHandler(PlayerSetup.Instance._animator.avatar, PlayerSetup.Instance._avatar.transform);
                    m_poseHandler.GetHumanPose(ref m_pose);

                    HumanPose l_tPose = new HumanPose
                    {
                        bodyPosition = m_pose.bodyPosition,
                        bodyRotation = m_pose.bodyRotation,
                        muscles = new float[m_pose.muscles.Length]
                    };
                    for(int i = 0; i < l_tPose.muscles.Length; i++)
                        l_tPose.muscles[i] = MusclePoses.TPoseMuscles[i];
                    m_poseHandler.SetHumanPose(ref l_tPose);
                }

                Transform l_hand = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftHand);
                if(l_hand != null)
                    m_leftHandTarget.localRotation = ms_offsetLeft * (PlayerSetup.Instance._avatar.transform.GetMatrix().inverse * l_hand.GetMatrix()).rotation;

                l_hand = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand);
                if(l_hand != null)
                    m_rightHandTarget.localRotation = ms_offsetRight * (PlayerSetup.Instance._avatar.transform.GetMatrix().inverse * l_hand.GetMatrix()).rotation;

                if(m_vrIK == null)
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

                    m_poseHandler?.SetHumanPose(ref m_pose);
                }
                else
                {
                    m_vrIK.onPreSolverUpdate.AddListener(this.OnIKPreUpdate);
                    m_vrIK.onPostSolverUpdate.AddListener(this.OnIKPostUpdate);
                }

                if(m_hips != null)
                    m_hips.localPosition = l_hipsPos;
            }
        }

        internal void OnAvatarReinitialize()
        {
            // Old VRIK is destroyed by game
            m_inVR = Utils.IsInVR();
            m_vrIK = PlayerSetup.Instance._animator.GetComponent<VRIK>();
            if(m_vrIK != null)
            {
                m_vrIK.onPreSolverUpdate.AddListener(this.OnIKPreUpdate);
                m_vrIK.onPostSolverUpdate.AddListener(this.OnIKPostUpdate);
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

        void RefreshArmIK()
        {
            if((m_leftArmIK != null) && (m_rightArmIK != null))
            {
                m_leftArmIK.enabled = (m_enabled && !m_fingersOnly);
                m_rightArmIK.enabled = (m_enabled && !m_fingersOnly);
            }
        }

        void UpdateFingers(LeapParser.LeapData p_data)
        {
            if(p_data.m_leftHand.m_present)
            {
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftThumb1Stretched, -0.5f - p_data.m_leftHand.m_bends[0]);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftThumb2Stretched, 0.7f - p_data.m_leftHand.m_bends[0] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftThumb3Stretched, 0.7f - p_data.m_leftHand.m_bends[0] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftThumbSpread, -p_data.m_leftHand.m_spreads[0]);

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftIndex1Stretched, 0.5f - p_data.m_leftHand.m_bends[1]);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftIndex2Stretched, 0.7f - p_data.m_leftHand.m_bends[1] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftIndex3Stretched, 0.7f - p_data.m_leftHand.m_bends[1] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftIndexSpread, p_data.m_leftHand.m_spreads[1]);

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftMiddle1Stretched, 0.5f - p_data.m_leftHand.m_bends[2]);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftMiddle2Stretched, 0.7f - p_data.m_leftHand.m_bends[2] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftMiddle3Stretched, 0.7f - p_data.m_leftHand.m_bends[2] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftMiddleSpread, p_data.m_leftHand.m_spreads[2]);

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftRing1Stretched, 0.5f - p_data.m_leftHand.m_bends[3]);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftRing2Stretched, 0.7f - p_data.m_leftHand.m_bends[3] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftRing3Stretched, 0.7f - p_data.m_leftHand.m_bends[3] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftRingSpread, -p_data.m_leftHand.m_spreads[3]);

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftLittle1Stretched, 0.5f - p_data.m_leftHand.m_bends[4]);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftLittle2Stretched, 0.7f - p_data.m_leftHand.m_bends[4] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftLittle3Stretched, 0.7f - p_data.m_leftHand.m_bends[4] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftLittleSpread, -p_data.m_leftHand.m_spreads[4]);
            }

            if(p_data.m_rightHand.m_present)
            {
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightThumb1Stretched, -0.5f - p_data.m_rightHand.m_bends[0]);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightThumb2Stretched, 0.7f - p_data.m_rightHand.m_bends[0] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightThumb3Stretched, 0.7f - p_data.m_rightHand.m_bends[0] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightThumbSpread, -p_data.m_rightHand.m_spreads[0]);

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightIndex1Stretched, 0.5f - p_data.m_rightHand.m_bends[1]);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightIndex2Stretched, 0.7f - p_data.m_rightHand.m_bends[1] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightIndex3Stretched, 0.7f - p_data.m_rightHand.m_bends[1] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightIndexSpread, p_data.m_rightHand.m_spreads[1]);

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightMiddle1Stretched, 0.5f - p_data.m_rightHand.m_bends[2]);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightMiddle2Stretched, 0.7f - p_data.m_rightHand.m_bends[2] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightMiddle3Stretched, 0.7f - p_data.m_rightHand.m_bends[2] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightMiddleSpread, p_data.m_rightHand.m_spreads[2]);

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightRing1Stretched, 0.5f - p_data.m_rightHand.m_bends[3]);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightRing2Stretched, 0.7f - p_data.m_rightHand.m_bends[3] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightRing3Stretched, 0.7f - p_data.m_rightHand.m_bends[3] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightRingSpread, -p_data.m_rightHand.m_spreads[3]);

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightLittle1Stretched, 0.5f - p_data.m_rightHand.m_bends[4]);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightLittle2Stretched, 0.7f - p_data.m_rightHand.m_bends[4] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightLittle3Stretched, 0.7f - p_data.m_rightHand.m_bends[4] * 2f);
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightLittleSpread, -p_data.m_rightHand.m_spreads[4]);
            }
        }

        static void UpdatePoseMuscle(ref HumanPose p_pose, int p_index, float p_value)
        {
            if(p_pose.muscles.Length > p_index)
                p_pose.muscles[p_index] = p_value;
        }
    }
}

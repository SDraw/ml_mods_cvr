using ABI_RC.Core.Player;
using ABI_RC.Systems.IK;
using RootMotion.FinalIK;
using System.Reflection;
using UnityEngine;

namespace ml_lme
{
    [DisallowMultipleComponent]
    class LeapTracked : MonoBehaviour
    {
        static readonly float[] ms_tposeMuscles = typeof(ABI_RC.Systems.IK.SubSystems.BodySystem).GetField("TPoseMuscles", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as float[];
        static readonly Quaternion ms_offsetLeft = Quaternion.Euler(0f, 0f, 270f);
        static readonly Quaternion ms_offsetRight = Quaternion.Euler(0f, 0f, 90f);
        static readonly Quaternion ms_offsetLeftDesktop = Quaternion.Euler(0f, 90f, 0f);
        static readonly Quaternion ms_offsetRightDesktop = Quaternion.Euler(0f, 270f, 0f);

        VRIK m_vrIK = null;
        Vector4 m_armsWeights = Vector2.zero;
        bool m_inVR = false;
        Transform m_hips = null;
        Transform m_origLeftHand = null;
        Transform m_origRightHand = null;
        Transform m_origLeftElbow = null;
        Transform m_origRightElbow = null;

        bool m_enabled = true;
        bool m_fingersOnly = false;
        bool m_trackElbows = true;

        ArmIK m_leftArmIK = null;
        ArmIK m_rightArmIK = null;
        HumanPoseHandler m_poseHandler = null;
        HumanPose m_pose;
        Transform m_leftHandTarget = null;
        Transform m_rightHandTarget = null;
        bool m_leftTargetActive = false;
        bool m_rightTargetActive = false;

        // Unity events
        void Start()
        {
            m_inVR = Utils.IsInVR();

            m_leftHandTarget = new GameObject("RotationTarget").transform;
            m_leftHandTarget.parent = LeapTracking.GetInstance().GetLeftHand();
            m_leftHandTarget.localPosition = Vector3.zero;
            m_leftHandTarget.localRotation = Quaternion.identity;

            m_rightHandTarget = new GameObject("RotationTarget").transform;
            m_rightHandTarget.parent = LeapTracking.GetInstance().GetRightHand();
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
            Settings.EnabledChange -= this.OnEnabledChange;
            Settings.FingersOnlyChange -= this.OnFingersOnlyChange;
            Settings.TrackElbowsChange -= this.OnTrackElbowsChange;
        }

        void Update()
        {
            if(m_enabled)
            {
                GestureMatcher.LeapData l_data = LeapManager.GetInstance().GetLatestData();

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
                    if(l_data.m_leftHand.m_present && !m_leftTargetActive)
                    {
                        m_vrIK.solver.leftArm.target = m_leftHandTarget;
                        m_vrIK.solver.leftArm.bendGoal = LeapTracking.GetInstance().GetLeftElbow();
                        m_vrIK.solver.leftArm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
                        m_leftTargetActive = true;
                    }
                    if(!l_data.m_leftHand.m_present && m_leftTargetActive)
                    {
                        m_vrIK.solver.leftArm.target = m_origLeftHand;
                        m_vrIK.solver.leftArm.bendGoal = m_origLeftElbow;
                        m_vrIK.solver.leftArm.bendGoalWeight = ((m_origLeftElbow != null) ? 1f : 0f);
                        m_leftTargetActive = false;
                    }

                    if(l_data.m_rightHand.m_present && !m_rightTargetActive)
                    {
                        m_vrIK.solver.rightArm.target = m_rightHandTarget;
                        m_vrIK.solver.rightArm.bendGoal = LeapTracking.GetInstance().GetRightElbow();
                        m_vrIK.solver.rightArm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
                        m_rightTargetActive = true;
                    }
                    if(!l_data.m_rightHand.m_present && m_rightTargetActive)
                    {
                        m_vrIK.solver.rightArm.target = m_origRightHand;
                        m_vrIK.solver.rightArm.bendGoal = m_origRightElbow;
                        m_vrIK.solver.rightArm.bendGoalWeight = ((m_origRightElbow != null) ? 1f : 0f);
                        m_rightTargetActive = false;
                    }
                }
            }
        }

        void LateUpdate()
        {
            if(m_enabled && !m_inVR && (m_poseHandler != null))
            {
                GestureMatcher.LeapData l_data = LeapManager.GetInstance().GetLatestData();

                Vector3 l_hipsLocalPos = m_hips.localPosition;
                Quaternion l_hipsLocalRot = m_hips.localRotation;

                m_poseHandler.GetHumanPose(ref m_pose);
                UpdateFingers(l_data);
                m_poseHandler.SetHumanPose(ref m_pose);

                m_hips.localPosition = l_hipsLocalPos;
                m_hips.localRotation = l_hipsLocalRot;
            }
        }

        // Tracking update
        void UpdateFingers(GestureMatcher.LeapData p_data)
        {
            if(p_data.m_leftHand.m_present)
            {
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftThumb1Stretched, Mathf.LerpUnclamped(0.85f, -0.85f, p_data.m_leftHand.m_bends[0]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftThumb2Stretched, Mathf.LerpUnclamped(0.85f, -0.85f, p_data.m_leftHand.m_bends[0]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftThumb3Stretched, Mathf.LerpUnclamped(0.85f, -0.85f, p_data.m_leftHand.m_bends[0]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftThumbSpread, Mathf.LerpUnclamped(-1.5f, 1.0f, p_data.m_leftHand.m_spreads[0])); // Ok

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftIndex1Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_leftHand.m_bends[1]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftIndex2Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_leftHand.m_bends[1]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftIndex3Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_leftHand.m_bends[1]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftIndexSpread, Mathf.LerpUnclamped(1f, -1f, p_data.m_leftHand.m_spreads[1])); // Ok

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftMiddle1Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_leftHand.m_bends[2]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftMiddle2Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_leftHand.m_bends[2]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftMiddle3Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_leftHand.m_bends[2]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftMiddleSpread, Mathf.LerpUnclamped(2f, -2f, p_data.m_leftHand.m_spreads[2]));

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftRing1Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_leftHand.m_bends[3]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftRing2Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_leftHand.m_bends[3]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftRing3Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_leftHand.m_bends[3]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftRingSpread, Mathf.LerpUnclamped(-2f, 2f, p_data.m_leftHand.m_spreads[3]));

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftLittle1Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_leftHand.m_bends[4]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftLittle2Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_leftHand.m_bends[4]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftLittle3Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_leftHand.m_bends[4]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftLittleSpread, Mathf.LerpUnclamped(-0.5f, 1f, p_data.m_leftHand.m_spreads[4]));
            }

            if(p_data.m_rightHand.m_present)
            {
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightThumb1Stretched, Mathf.LerpUnclamped(0.85f, -0.85f, p_data.m_rightHand.m_bends[0]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightThumb2Stretched, Mathf.LerpUnclamped(0.85f, -0.85f, p_data.m_rightHand.m_bends[0]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightThumb3Stretched, Mathf.LerpUnclamped(0.85f, -0.85f, p_data.m_rightHand.m_bends[0]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightThumbSpread, Mathf.LerpUnclamped(-1.5f, 1.0f, p_data.m_rightHand.m_spreads[0])); // Ok

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightIndex1Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_rightHand.m_bends[1]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightIndex2Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_rightHand.m_bends[1]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightIndex3Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_rightHand.m_bends[1]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightIndexSpread, Mathf.LerpUnclamped(1f, -1f, p_data.m_rightHand.m_spreads[1])); // Ok

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightMiddle1Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_rightHand.m_bends[2]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightMiddle2Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_rightHand.m_bends[2]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightMiddle3Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_rightHand.m_bends[2]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightMiddleSpread, Mathf.LerpUnclamped(2f, -2f, p_data.m_rightHand.m_spreads[2]));

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightRing1Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_rightHand.m_bends[3]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightRing2Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_rightHand.m_bends[3]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightRing3Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_rightHand.m_bends[3]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightRingSpread, Mathf.LerpUnclamped(-2f, 2f, p_data.m_rightHand.m_spreads[3]));

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightLittle1Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_rightHand.m_bends[4]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightLittle2Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_rightHand.m_bends[4]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightLittle3Stretched, Mathf.LerpUnclamped(0.7f, -1f, p_data.m_rightHand.m_bends[4]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightLittleSpread, Mathf.LerpUnclamped(-0.5f, 1f, p_data.m_rightHand.m_spreads[4]));
            }
        }

        // Game events
        internal void OnAvatarClear()
        {
            m_vrIK = null;
            m_origLeftHand = null;
            m_origRightHand = null;
            m_origLeftElbow = null;
            m_origRightElbow = null;
            m_hips = null;
            m_armsWeights = Vector2.zero;
            m_leftArmIK = null;
            m_rightArmIK = null;
            m_leftTargetActive = false;
            m_rightTargetActive = false;

            if(!m_inVR)
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
                        l_tPose.muscles[i] = ms_tposeMuscles[i];
                    m_poseHandler.SetHumanPose(ref l_tPose);
                }

                Transform l_hand = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftHand);
                if(l_hand != null)
                    m_leftHandTarget.localRotation = (m_inVR ? ms_offsetLeft : ms_offsetLeftDesktop) * (PlayerSetup.Instance._avatar.transform.GetMatrix().inverse * l_hand.GetMatrix()).rotation;

                l_hand = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand);
                if(l_hand != null)
                    m_rightHandTarget.localRotation = (m_inVR ? ms_offsetRight : ms_offsetRightDesktop) * (PlayerSetup.Instance._avatar.transform.GetMatrix().inverse * l_hand.GetMatrix()).rotation;

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
                    m_leftArmIK.solver.arm.bendGoal = LeapTracking.GetInstance().GetLeftElbow();
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
                    m_rightArmIK.solver.arm.bendGoal = LeapTracking.GetInstance().GetRightElbow();
                    m_rightArmIK.solver.arm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
                    m_rightArmIK.enabled = (m_enabled && !m_fingersOnly);

                    m_poseHandler?.SetHumanPose(ref m_pose);
                }
                else
                {
                    m_origLeftHand = m_vrIK.solver.leftArm.target;
                    m_origRightHand = m_vrIK.solver.rightArm.target;
                    m_origLeftElbow = m_vrIK.solver.leftArm.bendGoal;
                    m_origRightElbow = m_vrIK.solver.rightArm.bendGoal;
                    m_vrIK.solver.OnPreUpdate += this.OnIKPreUpdate;
                    m_vrIK.solver.OnPostUpdate += this.OnIKPostUpdate;
                }

                if(m_hips != null)
                    m_hips.localPosition = l_hipsPos;
            }
        }

        internal void OnCalibrate()
        {
            if(m_vrIK != null)
            {
                m_origLeftHand = m_vrIK.solver.leftArm.target;
                m_origRightHand = m_vrIK.solver.rightArm.target;
                m_origLeftElbow = m_vrIK.solver.leftArm.bendGoal;
                m_origRightElbow = m_vrIK.solver.rightArm.bendGoal;
            }
        }

        // IK updates
        void OnIKPreUpdate()
        {
            m_armsWeights.Set(
                m_vrIK.solver.leftArm.positionWeight,
                m_vrIK.solver.leftArm.rotationWeight,
                m_vrIK.solver.rightArm.positionWeight,
                m_vrIK.solver.rightArm.rotationWeight
            );

            if(m_leftTargetActive && (Mathf.Approximately(m_armsWeights.x, 0f) || Mathf.Approximately(m_armsWeights.y, 0f)))
            {
                m_vrIK.solver.leftArm.positionWeight = 1f;
                m_vrIK.solver.leftArm.rotationWeight = 1f;
            }
            if(m_rightTargetActive && (Mathf.Approximately(m_armsWeights.z, 0f) || Mathf.Approximately(m_armsWeights.w, 0f)))
            {
                m_vrIK.solver.rightArm.positionWeight = 1f;
                m_vrIK.solver.rightArm.rotationWeight = 1f;
            }
        }
        void OnIKPostUpdate()
        {
            m_vrIK.solver.leftArm.positionWeight = m_armsWeights.x;
            m_vrIK.solver.leftArm.rotationWeight = m_armsWeights.y;
            m_vrIK.solver.rightArm.positionWeight = m_armsWeights.z;
            m_vrIK.solver.rightArm.rotationWeight = m_armsWeights.w;
        }

        // Settings
        void OnEnabledChange(bool p_state)
        {
            m_enabled = p_state;

            RefreshArmIK();
            if(!m_enabled || m_fingersOnly)
                RestoreVRIK();
        }

        void OnFingersOnlyChange(bool p_state)
        {
            m_fingersOnly = p_state;

            RefreshArmIK();
            if(!m_enabled || m_fingersOnly)
                RestoreVRIK();
        }

        void OnTrackElbowsChange(bool p_state)
        {
            m_trackElbows = p_state;

            if((m_leftArmIK != null) && (m_rightArmIK != null))
            {
                m_leftArmIK.solver.arm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
                m_rightArmIK.solver.arm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
            }

            RestoreVRIK();
        }

        // Arbitrary
        void RestoreVRIK()
        {
            if(m_vrIK != null)
            {
                if(m_leftTargetActive)
                {
                    m_vrIK.solver.leftArm.target = m_origLeftHand;
                    m_vrIK.solver.leftArm.bendGoal = m_origLeftElbow;
                    m_vrIK.solver.leftArm.bendGoalWeight = ((m_origLeftElbow != null) ? 1f : 0f);
                    m_leftTargetActive = false;
                }
                if(m_rightTargetActive)
                {
                    m_vrIK.solver.rightArm.target = m_origRightHand;
                    m_vrIK.solver.rightArm.bendGoal = m_origRightElbow;
                    m_vrIK.solver.rightArm.bendGoalWeight = ((m_origRightElbow != null) ? 1f : 0f);
                    m_rightTargetActive = false;
                }
            }
        }

        void RefreshArmIK()
        {
            if((m_leftArmIK != null) && (m_rightArmIK != null))
            {
                m_leftArmIK.enabled = (m_enabled && !m_fingersOnly);
                m_rightArmIK.enabled = (m_enabled && !m_fingersOnly);
            }
        }

        static void UpdatePoseMuscle(ref HumanPose p_pose, int p_index, float p_value)
        {
            if(p_pose.muscles.Length > p_index)
                p_pose.muscles[p_index] = p_value;
        }
    }
}

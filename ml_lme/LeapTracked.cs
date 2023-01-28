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
        Vector2 m_armsWeights = Vector2.zero;
        bool m_inVR = false;
        Transform m_origElbowLeft = null;
        Transform m_origElbowRight = null;
        Transform m_hips = null;

        bool m_enabled = true;
        bool m_fingersOnly = false;
        bool m_trackElbows = true;

        ArmIK m_leftIK = null;
        ArmIK m_rightIK = null;
        HumanPoseHandler m_poseHandler = null;
        HumanPose m_pose;
        Transform m_leftHandTarget = null;
        Transform m_rightHandTarget = null;
        bool m_leftTargetActive = false;
        bool m_rightTargetActive = false;

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

            Settings.EnabledChange += this.SetEnabled;
            Settings.FingersOnlyChange += this.SetFingersOnly;
            Settings.TrackElbowsChange += this.SetTrackElbows;

            SetEnabled(Settings.Enabled);
            SetFingersOnly(Settings.FingersOnly);
            SetTrackElbows(Settings.TrackElbows);
        }

        void OnDestroy()
        {
            Settings.EnabledChange -= this.SetEnabled;
            Settings.FingersOnlyChange -= this.SetFingersOnly;
            Settings.TrackElbowsChange -= this.SetTrackElbows;
        }

        void SetEnabled(bool p_state)
        {
            m_enabled = p_state;

            RefreshArmIK();
            if(!m_enabled || m_fingersOnly)
                RestoreVRIK();
        }

        void SetFingersOnly(bool p_state)
        {
            m_fingersOnly = p_state;

            RefreshArmIK();
            if(!m_enabled || m_fingersOnly)
                RestoreVRIK();
        }

        void SetTrackElbows(bool p_state)
        {
            m_trackElbows = p_state;

            if((m_leftIK != null) && (m_rightIK != null))
            {
                m_leftIK.solver.arm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
                m_rightIK.solver.arm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
            }

            RestoreVRIK();
        }

        void Update()
        {
            if(m_enabled)
            {
                GestureMatcher.LeapData l_data = LeapManager.GetInstance().GetLatestData();

                if((m_leftIK != null) && (m_rightIK != null))
                {
                    m_leftIK.solver.IKPositionWeight = Mathf.Lerp(m_leftIK.solver.IKPositionWeight, (l_data.m_leftHand.m_present && !m_fingersOnly) ? 1f : 0f, 0.25f);
                    m_leftIK.solver.IKRotationWeight = Mathf.Lerp(m_leftIK.solver.IKRotationWeight, (l_data.m_leftHand.m_present && !m_fingersOnly) ? 1f : 0f, 0.25f);
                    if(m_trackElbows)
                        m_leftIK.solver.arm.bendGoalWeight = Mathf.Lerp(m_leftIK.solver.arm.bendGoalWeight, (l_data.m_leftHand.m_present && !m_fingersOnly) ? 1f : 0f, 0.25f);

                    m_rightIK.solver.IKPositionWeight = Mathf.Lerp(m_rightIK.solver.IKPositionWeight, (l_data.m_rightHand.m_present && !m_fingersOnly) ? 1f : 0f, 0.25f);
                    m_rightIK.solver.IKRotationWeight = Mathf.Lerp(m_rightIK.solver.IKRotationWeight, (l_data.m_rightHand.m_present && !m_fingersOnly) ? 1f : 0f, 0.25f);
                    if(m_trackElbows)
                        m_rightIK.solver.arm.bendGoalWeight = Mathf.Lerp(m_rightIK.solver.arm.bendGoalWeight, (l_data.m_rightHand.m_present && !m_fingersOnly) ? 1f : 0f, 0.25f);
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
                        m_vrIK.solver.leftArm.target = (m_inVR ? IKSystem.Instance.leftHandAnchor : null);
                        m_vrIK.solver.leftArm.bendGoal = m_origElbowLeft;
                        m_vrIK.solver.leftArm.bendGoalWeight = ((m_origElbowLeft != null) ? 1f : 0f);
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
                        m_vrIK.solver.rightArm.target = (m_inVR ? IKSystem.Instance.rightHandAnchor : null);
                        m_vrIK.solver.rightArm.bendGoal = m_origElbowRight;
                        m_vrIK.solver.rightArm.bendGoalWeight = ((m_origElbowRight != null) ? 1f : 0f);
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

                m_poseHandler.GetHumanPose(ref m_pose);
                UpdateFingers(l_data);
                m_poseHandler.SetHumanPose(ref m_pose);

                m_hips.localPosition = l_hipsLocalPos;
            }
        }

        void UpdateFingers(GestureMatcher.LeapData p_data)
        {
            if(p_data.m_leftHand.m_present)
            {
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftThumb1Stretched, Mathf.Lerp(0.85f, -0.85f, p_data.m_leftHand.m_bends[0]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftThumb2Stretched, Mathf.Lerp(0.85f, -0.85f, p_data.m_leftHand.m_bends[0]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftThumb3Stretched, Mathf.Lerp(0.85f, -0.85f, p_data.m_leftHand.m_bends[0]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftThumbSpread, Mathf.Lerp(-1.5f, 1.0f, p_data.m_leftHand.m_spreads[0])); // Ok

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftIndex1Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_leftHand.m_bends[1]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftIndex2Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_leftHand.m_bends[1]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftIndex3Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_leftHand.m_bends[1]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftIndexSpread, Mathf.Lerp(1f, -1f, p_data.m_leftHand.m_spreads[1])); // Ok

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftMiddle1Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_leftHand.m_bends[2]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftMiddle2Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_leftHand.m_bends[2]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftMiddle3Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_leftHand.m_bends[2]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftMiddleSpread, Mathf.Lerp(2f, -2f, p_data.m_leftHand.m_spreads[2]));

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftRing1Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_leftHand.m_bends[3]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftRing2Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_leftHand.m_bends[3]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftRing3Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_leftHand.m_bends[3]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftRingSpread, Mathf.Lerp(-2f, 2f, p_data.m_leftHand.m_spreads[3]));

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftLittle1Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_leftHand.m_bends[4]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftLittle2Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_leftHand.m_bends[4]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftLittle3Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_leftHand.m_bends[4]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.LeftLittleSpread, Mathf.Lerp(-0.5f, 1f, p_data.m_leftHand.m_spreads[4]));
            }

            if(p_data.m_rightHand.m_present)
            {
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightThumb1Stretched, Mathf.Lerp(0.85f, -0.85f, p_data.m_rightHand.m_bends[0]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightThumb2Stretched, Mathf.Lerp(0.85f, -0.85f, p_data.m_rightHand.m_bends[0]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightThumb3Stretched, Mathf.Lerp(0.85f, -0.85f, p_data.m_rightHand.m_bends[0]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightThumbSpread, Mathf.Lerp(-1.5f, 1.0f, p_data.m_rightHand.m_spreads[0])); // Ok

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightIndex1Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_rightHand.m_bends[1]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightIndex2Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_rightHand.m_bends[1]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightIndex3Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_rightHand.m_bends[1]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightIndexSpread, Mathf.Lerp(1f, -1f, p_data.m_rightHand.m_spreads[1])); // Ok

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightMiddle1Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_rightHand.m_bends[2]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightMiddle2Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_rightHand.m_bends[2]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightMiddle3Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_rightHand.m_bends[2]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightMiddleSpread, Mathf.Lerp(2f, -2f, p_data.m_rightHand.m_spreads[2]));

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightRing1Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_rightHand.m_bends[3]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightRing2Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_rightHand.m_bends[3]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightRing3Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_rightHand.m_bends[3]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightRingSpread, Mathf.Lerp(-2f, 2f, p_data.m_rightHand.m_spreads[3]));

                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightLittle1Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_rightHand.m_bends[4]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightLittle2Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_rightHand.m_bends[4]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightLittle3Stretched, Mathf.Lerp(0.7f, -1f, p_data.m_rightHand.m_bends[4]));
                UpdatePoseMuscle(ref m_pose, (int)MuscleIndex.RightLittleSpread, Mathf.Lerp(-0.5f, 1f, p_data.m_rightHand.m_spreads[4]));
            }
        }

        internal void OnAvatarClear()
        {
            m_vrIK = null;
            m_origElbowLeft = null;
            m_origElbowRight = null;
            m_hips = null;
            m_armsWeights = Vector2.zero;
            m_leftIK = null;
            m_rightIK = null;
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
                m_hips = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Hips);

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
                    m_leftHandTarget.localRotation = ((m_vrIK != null) ? ms_offsetLeft : ms_offsetLeftDesktop) * (PlayerSetup.Instance._avatar.transform.GetMatrix().inverse * l_hand.GetMatrix()).rotation;

                l_hand = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand);
                if(l_hand != null)
                    m_rightHandTarget.localRotation = ((m_vrIK != null) ? ms_offsetRight : ms_offsetRightDesktop) * (PlayerSetup.Instance._avatar.transform.GetMatrix().inverse * l_hand.GetMatrix()).rotation;

                if(m_vrIK == null)
                {
                    Transform l_chest = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.UpperChest);
                    if(l_chest == null)
                        l_chest = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Chest);
                    if(l_chest == null)
                        l_chest = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Spine);

                    m_leftIK = PlayerSetup.Instance._avatar.AddComponent<ArmIK>();
                    m_leftIK.solver.isLeft = true;
                    m_leftIK.solver.SetChain(
                        l_chest,
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftShoulder),
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftUpperArm),
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftLowerArm),
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftHand),
                        PlayerSetup.Instance._animator.transform
                    );
                    m_leftIK.solver.arm.target = m_leftHandTarget;
                    m_leftIK.solver.arm.bendGoal = LeapTracking.GetInstance().GetLeftElbow();
                    m_leftIK.solver.arm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
                    m_leftIK.enabled = (m_enabled && !m_fingersOnly);

                    m_rightIK = PlayerSetup.Instance._avatar.AddComponent<ArmIK>();
                    m_rightIK.solver.isLeft = false;
                    m_rightIK.solver.SetChain(
                        l_chest,
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightShoulder),
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightUpperArm),
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightLowerArm),
                        PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand),
                        PlayerSetup.Instance._animator.transform
                    );
                    m_rightIK.solver.arm.target = m_rightHandTarget;
                    m_rightIK.solver.arm.bendGoal = LeapTracking.GetInstance().GetRightElbow();
                    m_rightIK.solver.arm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
                    m_rightIK.enabled = (m_enabled && !m_fingersOnly);

                    m_poseHandler?.SetHumanPose(ref m_pose);
                }
                else
                {
                    m_vrIK.solver.OnPreUpdate += this.OnIKPreUpdate;
                    m_vrIK.solver.OnPostUpdate += this.OnIKPostUpdate;
                }
            }
        }

        internal void OnCalibrate()
        {
            if(m_vrIK != null)
            {
                m_origElbowLeft = m_vrIK.solver.leftArm.bendGoal;
                m_origElbowRight = m_vrIK.solver.rightArm.bendGoal;
            }
        }

        void OnIKPreUpdate()
        {
            m_armsWeights.Set(m_vrIK.solver.leftArm.positionWeight, m_vrIK.solver.rightArm.positionWeight);

            if(m_leftTargetActive && Mathf.Approximately(m_armsWeights.x, 0f))
            {
                m_vrIK.solver.leftArm.positionWeight = 1f;
                m_vrIK.solver.leftArm.rotationWeight = 1f;
            }
            if(m_rightTargetActive && Mathf.Approximately(m_armsWeights.y, 0f))
            {
                m_vrIK.solver.rightArm.positionWeight = 1f;
                m_vrIK.solver.rightArm.rotationWeight = 1f;
            }
        }
        void OnIKPostUpdate()
        {
            m_vrIK.solver.leftArm.positionWeight = m_armsWeights.x;
            m_vrIK.solver.leftArm.rotationWeight = m_armsWeights.x;

            m_vrIK.solver.rightArm.positionWeight = m_armsWeights.y;
            m_vrIK.solver.rightArm.rotationWeight = m_armsWeights.y;
        }

        void RestoreVRIK()
        {
            if(m_vrIK != null)
            {
                if(m_leftTargetActive)
                {
                    m_vrIK.solver.leftArm.target = (m_inVR ? IKSystem.Instance.leftHandAnchor : null);
                    m_vrIK.solver.leftArm.bendGoal = m_origElbowLeft;
                    m_vrIK.solver.leftArm.bendGoalWeight = ((m_origElbowLeft != null) ? 1f : 0f);
                    m_leftTargetActive = false;
                }
                if(m_rightTargetActive)
                {
                    m_vrIK.solver.rightArm.target = (m_inVR ? IKSystem.Instance.rightHandAnchor : null);
                    m_vrIK.solver.rightArm.bendGoal = m_origElbowRight;
                    m_vrIK.solver.rightArm.bendGoalWeight = ((m_origElbowRight != null) ? 1f : 0f);
                    m_rightTargetActive = false;
                }
            }
        }

        void RefreshArmIK()
        {
            if((m_leftIK != null) && (m_rightIK != null))
            {
                m_leftIK.enabled = (m_enabled && !m_fingersOnly);
                m_rightIK.enabled = (m_enabled && !m_fingersOnly);
            }
        }

        static void UpdatePoseMuscle(ref HumanPose p_pose, int p_index, float p_value)
        {
            if(p_pose.muscles.Length > p_index)
                p_pose.muscles[p_index] = p_value;
        }
    }
}

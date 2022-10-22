using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
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

        IndexIK m_indexIK = null;
        VRIK m_vrIK = null;
        Vector2 m_armsWeights = Vector2.zero;

        bool m_enabled = true;
        bool m_fingersOnly = false;
        bool m_trackElbows = true;

        ArmIK m_leftIK = null;
        ArmIK m_rightIK = null;
        Transform m_leftHand = null;
        Transform m_rightHand = null;
        Transform m_leftHandTarget = null;
        Transform m_rightHandTarget = null;
        Transform m_leftElbow = null;
        Transform m_rightElbow = null;
        bool m_leftTargetActive = false;
        bool m_rightTargetActive = false;

        void Start()
        {
            m_indexIK = this.GetComponent<IndexIK>();

            if(m_leftHand != null)
            {
                m_leftHandTarget = new GameObject("RotationTarget").transform;
                m_leftHandTarget.parent = m_leftHand;
                m_leftHandTarget.localPosition = Vector3.zero;
                m_leftHandTarget.localRotation = Quaternion.identity;
            }
            if(m_rightHand != null)
            {
                m_rightHandTarget = new GameObject("RotationTarget").transform;
                m_rightHandTarget.parent = m_rightHand;
                m_rightHandTarget.localPosition = Vector3.zero;
                m_rightHandTarget.localRotation = Quaternion.identity;
            }

            Settings.EnabledChange += this.SetEnabled;
            Settings.FingersOnlyChange += this.SetFingersOnly;
            Settings.TrackElbowsChange += this.SetTrackElbows;
        }

        void OnDestroy()
        {
            Settings.EnabledChange -= this.SetEnabled;
            Settings.FingersOnlyChange -= this.SetFingersOnly;
            Settings.TrackElbowsChange -= this.SetTrackElbows;
        }

        public void SetEnabled(bool p_state)
        {
            m_enabled = p_state;

            RefreshFingersTracking();
            RefreshArmIK();
            if(!m_enabled || m_fingersOnly)
                RestoreVRIK();
        }

        public void SetFingersOnly(bool p_state)
        {
            m_fingersOnly = p_state;

            RefreshArmIK();
            if(!m_enabled || m_fingersOnly)
                RestoreVRIK();
        }

        public void SetTrackElbows(bool p_state)
        {
            m_trackElbows = p_state;

            if((m_leftIK != null) && (m_rightIK != null))
            {
                m_leftIK.solver.arm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
                m_rightIK.solver.arm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
            }

            if(m_vrIK != null)
            {
                if(m_leftTargetActive)
                    m_vrIK.solver.leftArm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
                if(m_rightTargetActive)
                    m_vrIK.solver.rightArm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
            }
        }

        public void SetTransforms(Transform p_left, Transform p_right, Transform p_leftElbow, Transform p_rightElbow)
        {
            m_leftHand = p_left;
            m_rightHand = p_right;

            m_leftElbow = p_leftElbow;
            m_rightElbow = p_rightElbow;
        }

        public void UpdateTracking(GestureMatcher.GesturesData p_gesturesData)
        {
            if(m_enabled && (m_indexIK != null))
            {
                if((m_leftIK != null) && (m_rightIK != null))
                {
                    m_leftIK.solver.IKPositionWeight = Mathf.Lerp(m_leftIK.solver.IKPositionWeight, (p_gesturesData.m_handsPresenses[0] && !m_fingersOnly) ? 1f : 0f, 0.25f);
                    m_leftIK.solver.IKRotationWeight = Mathf.Lerp(m_leftIK.solver.IKRotationWeight, (p_gesturesData.m_handsPresenses[0] && !m_fingersOnly) ? 1f : 0f, 0.25f);
                    m_rightIK.solver.IKPositionWeight = Mathf.Lerp(m_rightIK.solver.IKPositionWeight, (p_gesturesData.m_handsPresenses[1] && !m_fingersOnly) ? 1f : 0f, 0.25f);
                    m_rightIK.solver.IKRotationWeight = Mathf.Lerp(m_rightIK.solver.IKRotationWeight, (p_gesturesData.m_handsPresenses[1] && !m_fingersOnly) ? 1f : 0f, 0.25f);
                }

                if(p_gesturesData.m_handsPresenses[0])
                {
                    m_indexIK.leftThumbCurl = p_gesturesData.m_leftFingersBends[0];
                    m_indexIK.leftIndexCurl = p_gesturesData.m_leftFingersBends[1];
                    m_indexIK.leftMiddleCurl = p_gesturesData.m_leftFingersBends[2];
                    m_indexIK.leftRingCurl = p_gesturesData.m_leftFingersBends[3];
                    m_indexIK.leftPinkyCurl = p_gesturesData.m_leftFingersBends[4];

                    if(CVRInputManager.Instance != null)
                    {
                        CVRInputManager.Instance.fingerCurlLeftThumb = p_gesturesData.m_leftFingersBends[0];
                        CVRInputManager.Instance.fingerCurlLeftIndex = p_gesturesData.m_leftFingersBends[1];
                        CVRInputManager.Instance.fingerCurlLeftMiddle = p_gesturesData.m_leftFingersBends[2];
                        CVRInputManager.Instance.fingerCurlLeftRing = p_gesturesData.m_leftFingersBends[3];
                        CVRInputManager.Instance.fingerCurlLeftPinky = p_gesturesData.m_leftFingersBends[4];
                    }
                }

                if(p_gesturesData.m_handsPresenses[1])
                {
                    m_indexIK.rightThumbCurl = p_gesturesData.m_rightFingersBends[0];
                    m_indexIK.rightIndexCurl = p_gesturesData.m_rightFingersBends[1];
                    m_indexIK.rightMiddleCurl = p_gesturesData.m_rightFingersBends[2];
                    m_indexIK.rightRingCurl = p_gesturesData.m_rightFingersBends[3];
                    m_indexIK.rightPinkyCurl = p_gesturesData.m_rightFingersBends[4];

                    if(CVRInputManager.Instance != null)
                    {
                        CVRInputManager.Instance.fingerCurlRightThumb = p_gesturesData.m_rightFingersBends[0];
                        CVRInputManager.Instance.fingerCurlRightIndex = p_gesturesData.m_rightFingersBends[1];
                        CVRInputManager.Instance.fingerCurlRightMiddle = p_gesturesData.m_rightFingersBends[2];
                        CVRInputManager.Instance.fingerCurlRightRing = p_gesturesData.m_rightFingersBends[3];
                        CVRInputManager.Instance.fingerCurlRightPinky = p_gesturesData.m_rightFingersBends[4];
                    }
                }

                if((m_vrIK != null) && !m_fingersOnly)
                {
                    if(p_gesturesData.m_handsPresenses[0] && !m_leftTargetActive)
                    {
                        m_vrIK.solver.leftArm.target = m_leftHandTarget;
                        m_vrIK.solver.leftArm.bendGoal = m_leftElbow;
                        m_vrIK.solver.leftArm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
                        m_leftTargetActive = true;
                    }
                    if(!p_gesturesData.m_handsPresenses[0] && m_leftTargetActive)
                    {
                        m_vrIK.solver.leftArm.target = IKSystem.Instance.leftHandAnchor;
                        m_vrIK.solver.leftArm.bendGoal = null;
                        m_vrIK.solver.leftArm.bendGoalWeight = 0f;
                        m_leftTargetActive = false;
                    }

                    if(p_gesturesData.m_handsPresenses[1] && !m_rightTargetActive)
                    {
                        m_vrIK.solver.rightArm.target = m_rightHandTarget;
                        m_vrIK.solver.rightArm.bendGoal = m_rightElbow;
                        m_vrIK.solver.rightArm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
                        m_rightTargetActive = true;
                    }
                    if(!p_gesturesData.m_handsPresenses[1] && m_rightTargetActive)
                    {
                        m_vrIK.solver.rightArm.target = IKSystem.Instance.rightHandAnchor;
                        m_vrIK.solver.rightArm.bendGoal = null;
                        m_vrIK.solver.rightArm.bendGoalWeight = 0f;
                        m_rightTargetActive = false;
                    }
                }
            }
        }

        public void OnAvatarClear()
        {
            m_vrIK = null;
            m_armsWeights = Vector2.zero;
            m_leftIK = null;
            m_rightIK = null;
            m_leftTargetActive = false;
            m_rightTargetActive = false;

            m_leftHandTarget.localPosition = Vector3.zero;
            m_leftHandTarget.localRotation = Quaternion.identity;
            m_rightHandTarget.localPosition = Vector3.zero;
            m_rightHandTarget.localRotation = Quaternion.identity;
        }

        public void OnCalibrateAvatar()
        {
            m_vrIK = PlayerSetup.Instance._animator.GetComponent<VRIK>();

            if(m_indexIK != null)
            {
                m_indexIK.avatarAnimator = PlayerSetup.Instance._animator;
                RefreshFingersTracking();
            }

            if(PlayerSetup.Instance._animator.isHuman)
            {
                HumanPoseHandler l_poseHandler = null;
                HumanPose l_initPose = new HumanPose();

                // Force desktop non-VRIK avatar into T-Pose
                if(m_vrIK == null)
                {
                    l_poseHandler = new HumanPoseHandler(PlayerSetup.Instance._animator.avatar, PlayerSetup.Instance._avatar.transform);
                    l_poseHandler.GetHumanPose(ref l_initPose);

                    HumanPose l_tPose = new HumanPose();
                    l_tPose.bodyPosition = l_initPose.bodyPosition;
                    l_tPose.bodyRotation = l_initPose.bodyRotation;
                    l_tPose.muscles = new float[l_initPose.muscles.Length];
                    for(int i = 0; i < l_tPose.muscles.Length; i++)
                        l_tPose.muscles[i] = ms_tposeMuscles[i];

                    l_poseHandler.SetHumanPose(ref l_tPose);
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
                    m_leftIK.solver.arm.bendGoal = m_leftElbow;
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
                    m_rightIK.solver.arm.bendGoal = m_rightElbow;
                    m_rightIK.solver.arm.bendGoalWeight = (m_trackElbows ? 1f : 0f);
                    m_rightIK.enabled = (m_enabled && !m_fingersOnly);

                    l_poseHandler.SetHumanPose(ref l_initPose);
                }
                else
                {
                    m_vrIK.solver.OnPreUpdate += this.OnIKPreUpdate;
                    m_vrIK.solver.OnPostUpdate += this.OnIKPostUpdate;
                }

                l_poseHandler?.Dispose();
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
                    m_vrIK.solver.leftArm.target = IKSystem.Instance.leftHandAnchor;
                    m_vrIK.solver.leftArm.bendGoal = null;
                    m_vrIK.solver.leftArm.bendGoalWeight = 0f;
                    m_leftTargetActive = false;
                }
                if(m_rightTargetActive)
                {
                    m_vrIK.solver.rightArm.target = IKSystem.Instance.rightHandAnchor;
                    m_vrIK.solver.rightArm.bendGoal = null;
                    m_vrIK.solver.rightArm.bendGoalWeight = 0f;
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

        void RefreshFingersTracking()
        {
            if(m_indexIK != null)
            {
                m_indexIK.activeControl = (m_enabled || Utils.AreKnucklesInUse());
                CVRInputManager.Instance.individualFingerTracking = (m_enabled || Utils.AreKnucklesInUse());
            }
        }
    }
}

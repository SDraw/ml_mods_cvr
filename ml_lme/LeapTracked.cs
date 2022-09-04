using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using RootMotion.FinalIK;
using UnityEngine;

namespace ml_lme
{
    [DisallowMultipleComponent]
    class LeapTracked : MonoBehaviour
    {
        static readonly Quaternion ms_offsetLeft = Quaternion.Euler(0f, 90f, 330f);
        static readonly Quaternion ms_offsetRight = Quaternion.Euler(0f, 270f, 30f);

        IndexIK m_indexIK = null;
        CVR_IK_Calibrator m_ikCalibrator = null;
        VRIK m_vrIK = null;

        bool m_enabled = true;
        bool m_fingersOnly = false;

        LeapIK m_leapIK = null;
        Transform m_leftHand = null;
        Transform m_rightHand = null;
        Transform m_leftHandTarget = null;
        Transform m_rightHandTarget = null;
        bool m_leftTargetActive = false;
        bool m_rightTargetActive = false;

        void Start()
        {
            m_indexIK = this.GetComponent<IndexIK>();
            m_ikCalibrator = this.GetComponent<CVR_IK_Calibrator>();

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
        }

        public void SetEnabled(bool p_state)
        {
            m_enabled = p_state;

            if(m_indexIK != null)
            {
                m_indexIK.activeControl = (m_enabled || Utils.AreKnucklesInUse());
                CVRInputManager.Instance.individualFingerTracking = (m_enabled || Utils.AreKnucklesInUse());
            }

            if(m_leapIK != null)
                m_leapIK.SetEnabled(m_enabled);

            if(!m_enabled || m_fingersOnly)
                RestoreIK();
        }

        public void SetFingersOnly(bool p_state)
        {
            m_fingersOnly = p_state;

            if(m_leapIK != null)
                m_leapIK.SetFingersOnly(m_fingersOnly);

            if(!m_enabled || m_fingersOnly)
                RestoreIK();
        }

        public void SetHands(Transform p_left, Transform p_right)
        {
            m_leftHand = p_left;
            m_rightHand = p_right;
        }

        public void UpdateTracking(GestureMatcher.GesturesData p_gesturesData)
        {
            if(m_enabled && (m_indexIK != null))
            {
                if(m_leapIK != null)
                    m_leapIK.SetHandsVisibility(p_gesturesData.m_handsPresenses[0], p_gesturesData.m_handsPresenses[1]);

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
                        m_leftTargetActive = true;
                    }
                    if(!p_gesturesData.m_handsPresenses[0] && m_leftTargetActive)
                    {
                        m_vrIK.solver.leftArm.target = m_ikCalibrator.leftHandAnchor;
                        m_leftTargetActive = false;
                    }

                    if(p_gesturesData.m_handsPresenses[1] && !m_rightTargetActive)
                    {
                        m_vrIK.solver.rightArm.target = m_rightHandTarget;
                        m_rightTargetActive = true;
                    }
                    if(!p_gesturesData.m_handsPresenses[1] && m_rightTargetActive)
                    {
                        m_vrIK.solver.rightArm.target = m_ikCalibrator.rightHandAnchor;
                        m_rightTargetActive = false;
                    }
                }
            }
        }

        public void OnAvatarClear()
        {
            m_leapIK = null;
            m_vrIK = null;
            m_leftTargetActive = false;
            m_rightTargetActive = false;
        }

        public void OnSetupAvatarGeneral()
        {
            m_vrIK = PlayerSetup.Instance._animator.GetComponent<VRIK>();

            if(m_indexIK != null)
            {
                m_indexIK.avatarAnimator = PlayerSetup.Instance._animator;
                m_indexIK.activeControl = (m_enabled || Utils.AreKnucklesInUse());
                CVRInputManager.Instance.individualFingerTracking = (m_enabled || Utils.AreKnucklesInUse());
            }

            if(!PlayerSetup.Instance._inVr)
            {
                m_leapIK = PlayerSetup.Instance._animator.gameObject.AddComponent<LeapIK>();
                m_leapIK.SetHands(m_leftHand, m_rightHand);
                m_leapIK.SetEnabled(m_enabled);
                m_leapIK.SetFingersOnly(m_fingersOnly);
            }

            if((m_vrIK != null) && PlayerSetup.Instance._animator.isHuman)
            {
                // Here we fokin' go!
                m_leftHand.position = Vector3.zero;
                m_leftHand.rotation = Quaternion.identity;
                Transform l_lowerArm = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                Transform l_hand = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftHand);
                if((l_lowerArm != null) && (l_hand != null))
                {
                    m_leftHandTarget.SetParent(l_lowerArm);
                    m_leftHandTarget.localPosition = l_hand.localPosition;
                    m_leftHandTarget.localRotation = l_hand.localRotation;
                    m_leftHandTarget.SetParent(m_leftHand, true);
                    m_leftHandTarget.localPosition = Vector3.zero;

                    Quaternion l_rot = m_leftHandTarget.localRotation;
                    m_leftHandTarget.localRotation = ms_offsetLeft * l_rot;
                }

                m_rightHand.position = Vector3.zero;
                m_rightHand.rotation = Quaternion.identity;
                l_lowerArm = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                l_hand = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand);
                if((l_lowerArm != null) && (l_hand != null))
                {
                    m_rightHandTarget.SetParent(l_lowerArm);
                    m_rightHandTarget.localPosition = l_hand.localPosition;
                    m_rightHandTarget.localRotation = l_hand.localRotation;
                    m_rightHandTarget.SetParent(m_rightHand, true);
                    m_rightHandTarget.localPosition = Vector3.zero;

                    Quaternion l_rot = m_rightHandTarget.localRotation;
                    m_rightHandTarget.localRotation = ms_offsetRight * l_rot;
                }
            }
        }

        void RestoreIK()
        {
            if((m_ikCalibrator != null) && (m_vrIK != null))
            {
                if(m_leftTargetActive)
                {
                    m_vrIK.solver.leftArm.target = m_ikCalibrator.leftHandAnchor;
                    m_leftTargetActive = false;
                }
                if(m_rightTargetActive)
                {
                    m_vrIK.solver.rightArm.target = m_ikCalibrator.rightHandAnchor;
                    m_rightTargetActive = false;
                }
            }
        }
    }
}

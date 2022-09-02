using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using RootMotion.FinalIK;
using System.Linq;
using UnityEngine;

namespace ml_lme
{
    [DisallowMultipleComponent]
    class LeapTracked : MonoBehaviour
    {
        bool m_enabled = true;
        bool m_fingersOnly = false;

        IndexIK m_indexIK = null;
        VRIK m_vrIK = null;

        LeapIK m_leapIK = null;
        Transform m_leftHand = null;
        Transform m_rightHand = null;

        bool m_knucklesInUse = false;

        void Start()
        {
            m_indexIK = this.GetComponent<IndexIK>();
            m_knucklesInUse = PlayerSetup.Instance._trackerManager.trackerNames.Contains("knuckles");

            if(PlayerSetup.Instance._inVr)
                PlayerSetup.Instance.avatarSetupCompleted.AddListener(this.OnAvatarSetup);
        }

        public void SetEnabled(bool p_state)
        {
            m_enabled = p_state;

            if(m_indexIK != null)
            {
                m_indexIK.activeControl = (m_enabled || m_knucklesInUse);
                CVRInputManager.Instance.individualFingerTracking = (m_enabled || m_knucklesInUse);
            }

            if(m_leapIK != null)
                m_leapIK.SetEnabled(m_enabled);
        }

        public void SetFingersOnly(bool p_state)
        {
            m_fingersOnly = p_state;

            if(m_leapIK != null)
                m_leapIK.SetFingersOnly(m_fingersOnly);
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
            }
        }

        public void UpdateTrackingLate(GestureMatcher.GesturesData p_gesturesData)
        {
            if(m_enabled && !m_fingersOnly && (m_vrIK != null) && m_vrIK.enabled)
            {
                if(p_gesturesData.m_handsPresenses[0])
                {
                    IKSolverVR.Arm l_arm = m_vrIK.solver?.leftArm;
                    if(l_arm?.target != null)
                    {
                        if(l_arm.positionWeight < 1f)
                            l_arm.positionWeight = 1f;
                        l_arm.target.position = p_gesturesData.m_handsPositons[0];

                        if(l_arm.rotationWeight < 1f)
                            l_arm.rotationWeight = 1f;
                        l_arm.target.rotation = p_gesturesData.m_handsRotations[0];
                    }
                }

                if(p_gesturesData.m_handsPresenses[1])
                {
                    IKSolverVR.Arm l_arm = m_vrIK.solver?.rightArm;
                    if(l_arm?.target != null)
                    {
                        if(l_arm.positionWeight < 1f)
                            l_arm.positionWeight = 1f;
                        l_arm.target.position = p_gesturesData.m_handsPositons[1];

                        if(l_arm.rotationWeight < 1f)
                            l_arm.rotationWeight = 1f;
                        l_arm.target.rotation = p_gesturesData.m_handsRotations[1];
                    }
                }
            }
        }

        public void OnAvatarClear()
        {
            m_leapIK = null;
            m_vrIK = null;
        }

        public void OnAvatarSetup()
        {
            m_knucklesInUse = PlayerSetup.Instance._trackerManager.trackerNames.Contains("knuckles");

            if(m_indexIK != null)
                m_indexIK.activeControl = (m_enabled || m_knucklesInUse);
            CVRInputManager.Instance.individualFingerTracking = (m_enabled || m_knucklesInUse);

            if(!PlayerSetup.Instance._inVr)
            {
                m_leapIK = PlayerSetup.Instance._animator.gameObject.AddComponent<LeapIK>();
                m_leapIK.SetEnabled(m_enabled);
                m_leapIK.SetFingersOnly(m_fingersOnly);
                m_leapIK.SetHands(m_leftHand, m_rightHand);
            }
            else
                m_vrIK = PlayerSetup.Instance._animator.GetComponent<VRIK>();
        }
    }
}

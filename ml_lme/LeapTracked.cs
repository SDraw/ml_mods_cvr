using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using System.Linq;
using UnityEngine;

namespace ml_lme
{
    [RequireComponent(typeof(IndexIK))]
    [DisallowMultipleComponent]
    class LeapTracked : MonoBehaviour
    {
        bool m_enabled = true;
        bool m_fingersOnly = false;
        bool m_calibrated = false;

        Animator m_animator = null;
        IndexIK m_indexIK = null;

        LeapIK m_leapIK = null;
        Transform m_leftHand = null;
        Transform m_rightHand = null;

        bool m_knucklesInUse = false;

        void Start()
        {
            m_indexIK = this.GetComponent<IndexIK>();
            m_knucklesInUse = PlayerSetup.Instance._trackerManager.trackerNames.Contains("knuckles");

            if((m_indexIK != null) && (m_animator != null))
            {
                if(!PlayerSetup.Instance._inVr)
                {
                    // Seems that VR mode always calibrates IndexIK, so let's force it
                    m_indexIK.avatarAnimator = m_animator;
                    m_indexIK.Recalibrate();
                }
                m_calibrated = true;

                m_indexIK.activeControl = (m_enabled || m_knucklesInUse);
                CVRInputManager.Instance.individualFingerTracking = (m_enabled || m_knucklesInUse);

                m_leapIK = m_animator.gameObject.AddComponent<LeapIK>();
                m_leapIK.SetEnabled(m_enabled);
                m_leapIK.SetFingersOnly(m_fingersOnly);
                m_leapIK.SetHands(m_leftHand, m_rightHand);
            }
        }

        public void SetEnabled(bool p_state)
        {
            m_enabled = p_state;
            if(m_enabled)
            {
                if((m_animator != null) && (m_indexIK != null))
                {
                    m_indexIK.activeControl = true;
                    if(!m_calibrated && !PlayerSetup.Instance._inVr)
                    {
                        m_indexIK.avatarAnimator = m_animator;
                        m_indexIK.Recalibrate();
                        m_calibrated = true;
                    }
                    CVRInputManager.Instance.individualFingerTracking = true;
                }
            }
            else
            {
                if((m_indexIK != null) && m_calibrated)
                {
                    m_indexIK.activeControl = m_knucklesInUse;
                    CVRInputManager.Instance.individualFingerTracking = m_knucklesInUse;
                }
            }

            if(m_leapIK != null)
                m_leapIK.SetEnabled(m_enabled);
        }

        public void SetAnimator(Animator p_animator) => m_animator = p_animator;

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
    }
}

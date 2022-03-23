using UnityEngine;

namespace ml_lme_cvr
{
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent]
    public class LeapIK : MonoBehaviour
    {
        bool m_enabled = true;
        bool m_fingersOnly = false;

        Animator m_animator = null;
        Transform m_leftHand = null;
        Transform m_rightHand = null;

        float m_leftHandWeight = 0f;
        float m_rightHandWeight = 0f;

        bool m_leftHandVisible = false;
        bool m_rightHandVisible = false;

        void Start()
        {
            m_animator = this.GetComponent<Animator>();
        }

        void OnAnimatorIK()
        {
            if(m_enabled && !m_fingersOnly && (m_animator != null))
            {
                if(m_leftHand != null)
                {
                    m_leftHandWeight = Mathf.Lerp(m_leftHandWeight, m_leftHandVisible ? 1f : 0f, 0.25f);
                    m_animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, m_leftHandWeight);
                    m_animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, m_leftHandWeight);
                    m_animator.SetIKPosition(AvatarIKGoal.LeftHand, m_leftHand.position);
                    m_animator.SetIKRotation(AvatarIKGoal.LeftHand, m_leftHand.rotation);
                }

                if(m_rightHand != null)
                {
                    m_rightHandWeight = Mathf.Lerp(m_rightHandWeight, m_rightHandVisible ? 1f : 0f, 0.25f);
                    m_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, m_rightHandWeight);
                    m_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, m_rightHandWeight);
                    m_animator.SetIKPosition(AvatarIKGoal.RightHand, m_rightHand.position);
                    m_animator.SetIKRotation(AvatarIKGoal.RightHand, m_rightHand.rotation);
                }
            }
        }

        public void SetEnabled(bool p_state) => m_enabled = p_state;

        public void SetFingersOnly(bool p_state) => m_fingersOnly = p_state;

        public void SetHands(Transform p_left, Transform p_right)
        {
            m_leftHand = p_left;
            m_rightHand = p_right;
        }

        public void SetHandsVisibility(bool p_left, bool p_right)
        {
            m_leftHandVisible = p_left;
            m_rightHandVisible = p_right;
        }
    }
}

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
                    m_animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
                    m_animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
                    m_animator.SetIKPosition(AvatarIKGoal.LeftHand, m_leftHand.position);
                    m_animator.SetIKRotation(AvatarIKGoal.LeftHand, m_leftHand.rotation);
                }

                if(m_rightHand != null)
                {
                    m_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
                    m_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
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
    }
}

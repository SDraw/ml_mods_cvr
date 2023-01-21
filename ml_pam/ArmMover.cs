using ABI.CCK.Components;
using ABI_RC.Core.Player;
using UnityEngine;

namespace ml_pam
{
    class ArmMover : MonoBehaviour
    {
        static readonly Vector4 ms_pointVector = new Vector4(0f, 0f, 0f, 1f);
        static readonly Quaternion ms_rotationOffset = Quaternion.Euler(0f, 0f, -90f);

        Animator m_animator = null;
        CVRPickupObject m_target = null;
        Matrix4x4 m_offset = Matrix4x4.identity;

        void Start()
        {
            m_animator = PlayerSetup.Instance._animator;
        }

        void OnAnimatorIK(int p_layerIndex)
        {
            if((p_layerIndex == 0) && (m_target != null)) // Only main Locomotion/Emotes layer
            {
                Transform l_camera = PlayerSetup.Instance.GetActiveCamera().transform;

                m_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
                m_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);

                switch(m_target.gripType)
                {
                    case CVRPickupObject.GripType.Origin:
                    {
                        if(m_target.gripOrigin != null)
                        {
                            m_animator.SetIKPosition(AvatarIKGoal.RightHand, m_target.gripOrigin.position);
                            m_animator.SetIKRotation(AvatarIKGoal.RightHand, l_camera.rotation * ms_rotationOffset);
                        }
                    }
                    break;

                    case CVRPickupObject.GripType.Free:
                    {
                        Matrix4x4 l_result = m_target.transform.GetMatrix() * m_offset;
                        m_animator.SetIKPosition(AvatarIKGoal.RightHand, l_result * ms_pointVector);
                        m_animator.SetIKRotation(AvatarIKGoal.RightHand, l_camera.rotation * ms_rotationOffset);
                    }
                    break;
                }
            }
        }

        public void SetTarget(CVRPickupObject p_target, Vector3 p_hit)
        {
            m_target = p_target;
            m_offset = (m_target != null) ? (p_target.transform.GetMatrix().inverse * Matrix4x4.Translate(p_hit)): Matrix4x4.identity;
        }
    }
}

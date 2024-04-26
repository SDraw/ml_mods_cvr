using ABI.CCK.Components;
using ABI_RC.Systems.Movement;
using UnityEngine;

namespace ml_prm
{
    [DisallowMultipleComponent]
    class GravityInfluencer : MonoBehaviour
    {
        Rigidbody m_rigidBody = null;
        PhysicsInfluencer m_physicsInfluencer = null;
        bool m_activeGravity = true;

        void Start()
        {
            m_rigidBody = this.GetComponent<Rigidbody>();
            m_physicsInfluencer = this.GetComponent<PhysicsInfluencer>();
        }

        void FixedUpdate()
        {
            m_rigidBody.useGravity = false;
            if(m_activeGravity && ((m_physicsInfluencer == null) || !m_physicsInfluencer.enableInfluence || !m_physicsInfluencer.GetSubmerged()))
                m_rigidBody.AddForce(BetterBetterCharacterController.Instance.GravityResult.AppliedGravity * m_rigidBody.mass);
        }

        public void SetActiveGravity(bool p_state) => m_activeGravity = p_state;
    }
}

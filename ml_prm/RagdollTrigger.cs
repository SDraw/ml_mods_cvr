using ABI.CCK.Components;
using UnityEngine;

namespace ml_prm
{
    [DisallowMultipleComponent]
    class RagdollTrigger : MonoBehaviour
    {
        static int ms_localPlayerLayer = 0;

        Collider m_lastCollider = null;
        bool m_triggered = false;

        void Start()
        {
            ms_localPlayerLayer = LayerMask.NameToLayer("PlayerLocal");
        }

        void OnTriggerEnter(Collider p_other)
        {
            CVRPointer l_pointer = p_other.gameObject.GetComponent<CVRPointer>();
            if((l_pointer != null) && l_pointer.type == "ragdoll" && p_other.gameObject.layer == ms_localPlayerLayer && (m_lastCollider != p_other))
            {
                m_lastCollider = p_other;
                m_triggered = true;
            }
        }

        void OnTriggerExit(Collider p_other)
        {
            if(m_lastCollider == p_other)
                m_lastCollider = null;
        }

        public bool GetStateWithReset()
        {
            bool l_state = m_triggered;
            m_triggered = false;
            return l_state;
        }
    }
}

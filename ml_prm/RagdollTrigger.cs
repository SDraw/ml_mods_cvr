using ABI.CCK.Components;
using ABI_RC.Core.Player;
using UnityEngine;

namespace ml_prm
{
    [DisallowMultipleComponent]
    class RagdollTrigger : MonoBehaviour
    {
        Collider m_collider = null;
        Collider m_lastTrigger = null;
        bool m_triggered = false;

        void Start()
        {
            m_collider = this.GetComponent<Collider>();
        }

        void Update()
        {
            if(!ReferenceEquals(m_lastTrigger, null))
            {
                if(m_lastTrigger != null)
                {
                    if(!m_collider.bounds.Intersects(m_lastTrigger.bounds))
                        m_lastTrigger = null;
                }
                else
                    m_lastTrigger = null;
            }
        }

        void OnTriggerEnter(Collider p_other)
        {
            CVRPointer l_pointer = p_other.GetComponent<CVRPointer>();
            if((l_pointer != null) && (l_pointer.type == "ragdoll") && !IsIgnored(l_pointer.transform) && (m_lastTrigger != p_other))
            {
                m_lastTrigger = p_other;
                m_triggered = true;
            }
        }

        void OnTriggerExit(Collider p_other)
        {
            if(m_lastTrigger == p_other)
                m_lastTrigger = null;
        }

        public bool GetStateWithReset()
        {
            bool l_state = m_triggered;
            m_triggered = false;
            return l_state;
        }

        static bool IsIgnored(Transform p_transform)
        {
            return (Settings.IgnoreLocal && (p_transform.root == PlayerSetup.Instance.transform));
        }
    }
}

using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Player;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ml_prm
{
    class RemoteGestureHandler : MonoBehaviour
    {
        internal class GestureEvent<T1, T2, T3>
        {
            event Action<T1, T2, T3> m_action;
            public void AddHandler(Action<T1, T2, T3> p_listener) => m_action += p_listener;
            public void RemoveHandler(Action<T1, T2, T3> p_listener) => m_action -= p_listener;
            public void Invoke(T1 p_objA, T2 p_objB, T3 p_objC) => m_action?.Invoke(p_objA, p_objB, p_objC);
        }

        public static readonly GestureEvent<PuppetMaster, bool, bool> OnGestureState = new GestureEvent<PuppetMaster, bool, bool>();

        PuppetMaster m_puppetMaster = null;
        bool m_stateLeft = false;
        bool m_stateRight = false;

        void Start()
        {
            m_puppetMaster = this.GetComponent<PuppetMaster>();
        }

        void Update()
        {
            bool l_state = Mathf.Approximately(m_puppetMaster.PlayerAvatarMovementDataInput.AnimatorGestureLeft, 1f);
            if(m_stateLeft != l_state)
            {
                m_stateLeft = l_state;
                if(!Settings.FriendsGrab || Friends.FriendsWith(m_puppetMaster.CVRPlayerEntity.PlayerDescriptor.ownerId))
                    OnGestureState.Invoke(m_puppetMaster, true, m_stateLeft);
            }

            l_state = Mathf.Approximately(m_puppetMaster.PlayerAvatarMovementDataInput.AnimatorGestureRight, 1f);
            if(m_stateRight != l_state)
            {
                m_stateRight = l_state;
                if(!Settings.FriendsGrab || Friends.FriendsWith(m_puppetMaster.CVRPlayerEntity.PlayerDescriptor.ownerId))
                    OnGestureState.Invoke(m_puppetMaster, false, m_stateRight);
            }
        }
    }
}

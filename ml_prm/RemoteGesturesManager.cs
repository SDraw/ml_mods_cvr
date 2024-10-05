using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Player;
using ABI_RC.Systems.GameEventSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ml_prm
{
    [DisallowMultipleComponent]
    class RemoteGesturesManager : MonoBehaviour
    {
        public enum GestureHand
        {
            Left = 0,
            Right
        }
        internal class GestureEvent<T1, T2, T3>
        {
            event Action<T1, T2, T3> m_action;
            public void AddListener(Action<T1, T2, T3> p_listener) => m_action += p_listener;
            public void RemoveListener(Action<T1, T2, T3> p_listener) => m_action -= p_listener;
            public void Invoke(T1 p_objA, T2 p_objB, T3 p_objC) => m_action?.Invoke(p_objA, p_objB, p_objC);
        }

        public static readonly GestureEvent<PuppetMaster, GestureHand, bool> OnGestureState = new GestureEvent<PuppetMaster, GestureHand, bool>();

        class PlayerEntry
        {
            public CVRPlayerEntity m_entity = null;
            public PuppetMaster m_puppetMaster = null;
            public bool m_stateLeft = false;
            public bool m_stateRight = false;
        }

        static RemoteGesturesManager ms_instance = null;

        readonly List<PlayerEntry> m_entries = null;

        internal RemoteGesturesManager()
        {
            m_entries = new List<PlayerEntry>();
        }

        void Awake()
        {
            if(ms_instance != null)
            {
                DestroyImmediate(this);
                return;
            }

            ms_instance = this;
            DontDestroyOnLoad(this);
        }

        void Start()
        {
            CVRGameEventSystem.Player.OnJoinEntity.AddListener(OnRemotePlayerCreated);
            CVRGameEventSystem.Player.OnLeaveEntity.AddListener(OnRemotePlayerDestroyed);
            Settings.OnGestureGrabChanged.AddListener(OnGestureGrabChanged);
        }

        void OnDestroy()
        {
            if(ms_instance == this)
                ms_instance = null;

            m_entries.Clear();

            CVRGameEventSystem.Player.OnJoinEntity.RemoveListener(OnRemotePlayerCreated);
            CVRGameEventSystem.Player.OnLeaveEntity.RemoveListener(OnRemotePlayerDestroyed);
            Settings.OnGestureGrabChanged.RemoveListener(OnGestureGrabChanged);
        }

        void Update()
        {
            if(Settings.GestureGrab)
            {
                foreach(var l_entry in m_entries)
                {
                    bool l_state = l_entry.m_puppetMaster.IsLeftGrabPointerActive();
                    if(l_entry.m_stateLeft != l_state)
                    {
                        l_entry.m_stateLeft = l_state;
                        if(!Settings.FriendsGrab || Friends.FriendsWith(l_entry.m_entity.PlayerDescriptor.ownerId))
                            OnGestureState.Invoke(l_entry.m_puppetMaster, GestureHand.Left, l_entry.m_stateLeft);
                    }

                    l_state = l_entry.m_puppetMaster.IsRightGrabPointerActive();
                    if(l_entry.m_stateRight != l_state)
                    {
                        l_entry.m_stateRight = l_state;
                        if(!Settings.FriendsGrab || Friends.FriendsWith(l_entry.m_entity.PlayerDescriptor.ownerId))
                            OnGestureState.Invoke(l_entry.m_puppetMaster, GestureHand.Right, l_entry.m_stateRight);
                    }
                }
            }
        }

        void OnRemotePlayerCreated(CVRPlayerEntity p_player)
        {
            try
            {
                if((p_player != null) && (p_player.PuppetMaster != null))
                {
                    PlayerEntry l_entry = new PlayerEntry()
                    {
                        m_entity = p_player,
                        m_puppetMaster = p_player.PuppetMaster,
                        m_stateLeft = false,
                        m_stateRight = false
                    };
                    m_entries.Add(l_entry);
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        void OnRemotePlayerDestroyed(CVRPlayerEntity p_player)
        {
            try
            {
                if(p_player != null)
                    m_entries.RemoveAll(e => ReferenceEquals(e.m_puppetMaster, p_player.PuppetMaster));
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        void OnGestureGrabChanged(bool p_state)
        {
            if(!p_state)
            {
                foreach(var l_entry in m_entries)
                {
                    l_entry.m_stateLeft = false;
                    l_entry.m_stateRight = false;
                }
            }
        }
    }
}

using ABI_RC.Core.Player;
using ABI_RC.Systems.GameEventSystem;
using System;
using System.Collections.Generic;

namespace ml_prm
{
    static class RemoteGestureManager
    {
        static readonly Dictionary<CVRPlayerEntity, RemoteGestureHandler> ms_remoteHandlers = new Dictionary<CVRPlayerEntity, RemoteGestureHandler>();

        internal static void Init()
        {
            CVRGameEventSystem.Player.OnJoinEntity.AddListener(OnRemotePlayerCreated);
            CVRGameEventSystem.Player.OnLeaveEntity.AddListener(OnRemotePlayerDestroyed);
            Settings.OnGestureGrabChanged.AddListener(OnGestureGrabChanged);
        }

        internal static void DeInit()
        {
            CVRGameEventSystem.Player.OnJoinEntity.RemoveListener(OnRemotePlayerCreated);
            CVRGameEventSystem.Player.OnLeaveEntity.RemoveListener(OnRemotePlayerDestroyed);
            Settings.OnGestureGrabChanged.RemoveListener(OnGestureGrabChanged);
        }

        static void OnRemotePlayerCreated(CVRPlayerEntity p_player)
        {
            try
            {
                if(Settings.GestureGrab && (p_player != null) && (p_player.PuppetMaster != null))
                {
                    RemoteGestureHandler l_handler = p_player.PuppetMaster.gameObject.AddComponent<RemoteGestureHandler>();
                    ms_remoteHandlers.Add(p_player, l_handler);
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnRemotePlayerDestroyed(CVRPlayerEntity p_player)
        {
            try
            {
                if(p_player != null)
                    ms_remoteHandlers.Remove(p_player);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnGestureGrabChanged(bool p_state)
        {
            if(p_state)
            {
                foreach(var l_player in CVRPlayerManager.Instance.NetworkPlayers)
                {
                    if(!ms_remoteHandlers.ContainsKey(l_player) && (l_player.PuppetMaster != null))
                    {
                        RemoteGestureHandler l_handler = l_player.PuppetMaster.gameObject.AddComponent<RemoteGestureHandler>();
                        ms_remoteHandlers.Add(l_player, l_handler);
                    }
                }
            }
            else
            {
                foreach(var l_pair in ms_remoteHandlers)
                {
                    if(l_pair.Value != null)
                        UnityEngine.Object.Destroy(l_pair.Value);
                }
                ms_remoteHandlers.Clear();
            }
        }
    }
}

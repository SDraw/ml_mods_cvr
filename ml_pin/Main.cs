using ABI_RC.Core.AudioEffects;
using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Core.Networking.IO.Instancing;
using ABI_RC.Systems.GameEventSystem;
using System;
using System.Collections;

namespace ml_pin
{
    public class PlayersInstanceNotifier : MelonLoader.MelonMod
    {
        SoundManager m_soundManager = null;

        public override void OnInitializeMelon()
        {
            ResourcesHandler.ExtractAudioResources();
        }

        public override void OnLateInitializeMelon()
        {
            Settings.Init();
            MelonLoader.MelonCoroutines.Start(WaitForInstances());
        }

        IEnumerator WaitForInstances()
        {
            if(InterfaceAudio.Instance == null)
                yield return null;

            m_soundManager = new SoundManager();
            m_soundManager.LoadSounds();

            CVRGameEventSystem.Player.OnJoinEntity.AddListener(OnPlayerJoin);
            CVRGameEventSystem.Player.OnLeaveEntity.AddListener(OnPlayerLeave);
        }

        public override void OnDeinitializeMelon()
        {
            m_soundManager = null;

            CVRGameEventSystem.Player.OnJoinEntity.RemoveListener(OnPlayerJoin);
            CVRGameEventSystem.Player.OnLeaveEntity.RemoveListener(OnPlayerLeave);
        }

        void OnPlayerJoin(CVRPlayerEntity p_player)
        {
            try
            {
                if((p_player != null) && (p_player.PlayerDescriptor != null)) // This happens sometimes, no idea why
                {
                    bool l_isFriend = Friends.FriendsWith(p_player.PlayerDescriptor.ownerId);
                    bool l_notify = false;

                    switch(Settings.NotifyType)
                    {
                        case Settings.NotificationType.None:
                            l_notify = false;
                            break;
                        case Settings.NotificationType.Friends:
                            l_notify = (l_isFriend && ShouldNotifyInCurrentInstance());
                            break;
                        case Settings.NotificationType.All:
                            l_notify = ShouldNotifyInCurrentInstance();
                            break;
                    }
                    l_notify |= (l_isFriend && Settings.FriendsAlways);

                    if(l_notify)
                        m_soundManager?.PlaySound(l_isFriend ? SoundManager.SoundType.FriendJoin : SoundManager.SoundType.PlayerJoin);
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Warning(e);
            }
        }
        void OnPlayerLeave(CVRPlayerEntity p_player)
        {
            try
            {
                if((p_player != null) && (p_player.PlayerDescriptor != null)) // This happens sometimes, no idea why
                {
                    bool l_isFriend = Friends.FriendsWith(p_player.PlayerDescriptor.ownerId);
                    bool l_notify = false;

                    switch(Settings.NotifyType)
                    {
                        case Settings.NotificationType.None:
                            l_notify = false;
                            break;
                        case Settings.NotificationType.Friends:
                            l_notify = (l_isFriend && ShouldNotifyInCurrentInstance());
                            break;
                        case Settings.NotificationType.All:
                            l_notify = ShouldNotifyInCurrentInstance();
                            break;
                    }
                    l_notify |= (l_isFriend && Settings.FriendsAlways);

                    if(l_notify)
                        m_soundManager?.PlaySound(l_isFriend ? SoundManager.SoundType.FriendLeave : SoundManager.SoundType.PlayerLeave);
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Warning(e);
            }
        }

        bool ShouldNotifyInCurrentInstance()
        {
            bool l_isInPublic = ((MetaPort.Instance.CurrentInstancePrivacyType == Instances.InstancePrivacyType.Public) && Settings.NotifyInPublic);
            bool l_isInFriends = (((MetaPort.Instance.CurrentInstancePrivacyType == Instances.InstancePrivacyType.Friends) || (MetaPort.Instance.CurrentInstancePrivacyType == Instances.InstancePrivacyType.FriendsOfFriends)) && Settings.NotifyInFriends);
            bool l_isInPrivate = (((MetaPort.Instance.CurrentInstancePrivacyType == Instances.InstancePrivacyType.EveryoneCanInvite) || (MetaPort.Instance.CurrentInstancePrivacyType == Instances.InstancePrivacyType.OwnerMustInvite)) && Settings.NotifyInPrivate);
            return (l_isInPublic || l_isInFriends || l_isInPrivate);
        }
    }
}

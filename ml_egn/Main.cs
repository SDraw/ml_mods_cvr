using ABI_RC.Core.EventSystem;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Networking;
using ABI_RC.Core.Util;
using DarkRift.Client;
using System.Reflection;

namespace ml_egn
{
    public class ExtendedGameNotifications : MelonLoader.MelonMod
    {
        public override void OnInitializeMelon()
        {
            HarmonyInstance.Patch(
                typeof(AssetManagement).GetMethod(nameof(AssetManagement.LoadLocalAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(ExtendedGameNotifications).GetMethod(nameof(OnLocalAvatarLoad), BindingFlags.NonPublic | BindingFlags.Static))
            );

            HarmonyInstance.Patch(
                typeof(CVRSyncHelper).GetMethod(nameof(CVRSyncHelper.SpawnProp)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(ExtendedGameNotifications).GetMethod(nameof(OnPropSpawned), BindingFlags.NonPublic | BindingFlags.Static))
            );

            HarmonyInstance.Patch(
                typeof(NetworkManager).GetMethod("OnGameNetworkConnectionClosed", BindingFlags.NonPublic | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(ExtendedGameNotifications).GetMethod(nameof(OnGameNetworkConnectionClosed), BindingFlags.NonPublic | BindingFlags.Static))
            );
        }

        static void OnLocalAvatarLoad()
        {
            try
            {
                if(Utils.IsMenuOpened())
                    Utils.ShowMenuNotification("Avatar changed", 1f);
                else
                    Utils.ShowHUDNotification("(Synced) Client", "Avatar changed");
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnPropSpawned()
        {
            try
            {
                if(Utils.IsConnected())
                {
                    if(Utils.IsMenuOpened())
                        Utils.ShowMenuNotification("Prop spawned", 1f);
                    else
                        Utils.ShowHUDNotification("(Synced) Client", "Prop spawned");
                }
                else
                {
                    if(Utils.IsMenuOpened())
                        Utils.ShowMenuAlert("Prop Error", "Not connected to live instance");
                    else
                        Utils.ShowHUDNotification("(Local) Client", "Unable to spawn prop", "Not connected to live instance");
                }
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnGameNetworkConnectionClosed(DisconnectedEventArgs __1)
        {
            try
            {
                if((__1 != null) && (!__1.LocalDisconnect))
                    Utils.ShowHUDNotification("(Local) Client", "Connection lost", (__1.Error != System.Net.Sockets.SocketError.Success) ? ("Reason: " + __1.Error.ToString()) : "", true);
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

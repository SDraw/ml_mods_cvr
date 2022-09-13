using ABI_RC.Core.EventSystem;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Networking;
using ABI_RC.Core.Util;
using DarkRift;

namespace ml_aci
{
    public class AvatarChangeInfo : MelonLoader.MelonMod
    {
        public override void OnApplicationStart()
        {
            HarmonyInstance.Patch(
                typeof(AssetManagement).GetMethod(nameof(AssetManagement.LoadLocalAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarChangeInfo).GetMethod(nameof(OnLocalAvatarLoad), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static))
            );

            HarmonyInstance.Patch(
                typeof(CVRSyncHelper).GetMethod(nameof(CVRSyncHelper.SpawnProp)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarChangeInfo).GetMethod(nameof(OnPropSpawned), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static))
            );
        }

        static void OnLocalAvatarLoad()
        {
            try
            {
                if(ViewManager.Instance != null)
                    ViewManager.Instance.TriggerPushNotification("Avatar changed", 1f);
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
                if(ViewManager.Instance != null)
                {
                    if((NetworkManager.Instance != null) && (NetworkManager.Instance.GameNetwork.ConnectionState == ConnectionState.Connected))
                        ViewManager.Instance.TriggerPushNotification("Prop spawned", 1f);
                    else
                        ViewManager.Instance.TriggerAlert("Prop Error", "Not connected to live instance", -1, true);
                }
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

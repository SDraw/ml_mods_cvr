using ABI_RC.Core.Player;
using System.Reflection;

namespace ml_asl
{
    public class AvatarSyncedLook : MelonLoader.MelonMod
    {
        public override void OnInitializeMelon()
        {
            Settings.Init();

            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod("UpdatePlayerAvatarMovementData", BindingFlags.NonPublic | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarSyncedLook).GetMethod(nameof(OnPlayerAvatarMovementDataUpdate_Postfix), BindingFlags.NonPublic | BindingFlags.Static))
            );
        }

        static void OnPlayerAvatarMovementDataUpdate_Postfix(ref PlayerSetup __instance, PlayerAvatarMovementData ____playerAvatarMovementData)
        {
            if(Settings.Enabled && (__instance.EyeMovementController != null))
                ____playerAvatarMovementData.EyeTrackingOverride = true;
        }
    }
}

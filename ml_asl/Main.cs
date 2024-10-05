using ABI_RC.Core.Player;
using System.Reflection;
using UnityEngine;

namespace ml_asl
{
    public class AvatarSyncedLook : MelonLoader.MelonMod
    {
        readonly static Matrix4x4 ms_back = Matrix4x4.Translate(Vector3.back);

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
            {
                ____playerAvatarMovementData.EyeTrackingOverride = true;

                if(__instance.EyeMovementController.CurrentTarget != null)
                    ____playerAvatarMovementData.EyeTrackingPosition = __instance.EyeMovementController.CurrentTarget.GetPosition();
                else
                    ____playerAvatarMovementData.EyeTrackingPosition = (__instance.transform.GetMatrix() * ms_back).GetPosition();
            }
        }
    }
}

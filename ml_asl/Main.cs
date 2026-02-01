using ABI_RC.Core.Player;
using ABI_RC.Systems.FaceTracking;
using System;
using System.Reflection;
using UnityEngine;

namespace ml_asl
{
    public class AvatarSyncedLook : MelonLoader.MelonMod
    {
        readonly static Matrix4x4 ms_back = Matrix4x4.Translate(Vector3.back);

        public override void OnInitializeMelon()
        {
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod("UpdatePlayerAvatarMovementData", BindingFlags.Instance | BindingFlags.NonPublic),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarSyncedLook).GetMethod(nameof(OnPlayerAvatarMovementDataUpdate_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
        }

        public override void OnLateInitializeMelon()
        {
            Settings.Init();
        }

        static void OnPlayerAvatarMovementDataUpdate_Postfix(ref PlayerSetup __instance, PlayerAvatarMovementData ____playerAvatarMovementData)
        {
            try
            {
                if(Settings.Enabled && (__instance.EyeMovementController != null) && !FaceTrackingManager.Instance.IsEyeDataAvailable())
                {
                    ____playerAvatarMovementData.EyeTrackingOverride = true;

                    if(__instance.EyeMovementController.CurrentTarget != null)
                        ____playerAvatarMovementData.EyeTrackingPosition = __instance.EyeMovementController.CurrentTarget.GetPosition();
                    else
                        ____playerAvatarMovementData.EyeTrackingPosition = (__instance.transform.GetMatrix() * ms_back).GetPosition();
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

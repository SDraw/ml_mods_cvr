using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.FaceTracking;
using System;
using System.Reflection;
using UnityEngine;

namespace ml_asl
{
    public class AvatarSyncedLook : MelonLoader.MelonMod
    {
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
                if(Settings.Enabled && (__instance.EyeMovementController != null))
                {
                    if(!FaceTrackingManager.Instance.IsEyeDataAvailable())
                    {
                        ____playerAvatarMovementData.EyeTrackingOverride = true;

                        if(__instance.EyeMovementController.CurrentTarget != null)
                            ____playerAvatarMovementData.EyeTrackingPosition = __instance.EyeMovementController.CurrentTarget.GetPosition();
                        else
                        {
                            Transform l_camera = (!MetaPort.Instance.isUsingVr ? __instance.desktopCamera : __instance.vrCamera).transform;
                            ____playerAvatarMovementData.EyeTrackingPosition = l_camera.position - l_camera.forward;
                        }

                        ____playerAvatarMovementData.EyeBlinkingOverride = true;
                        ____playerAvatarMovementData.EyeTrackingBlinkProgressLeft = __instance.EyeMovementController.blinkProgressLeft;
                        ____playerAvatarMovementData.EyeTrackingBlinkProgressRight = __instance.EyeMovementController.blinkProgressRight;
                    }
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

using System;
using System.Reflection;
using ABI_RC.Systems.InputManagement;
using ABI_RC.Systems.InputManagement.XR;

namespace ml_vgm
{
    public class ViveGesturesMovement : MelonLoader.MelonMod
    {
        public override void OnInitializeMelon()
        {
            HarmonyInstance.Patch(
                typeof(CVRXRModule).GetMethod("Update_Gestures_Vive", BindingFlags.NonPublic | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(ViveGesturesMovement).GetMethod(nameof(OnViveGesturesUpdate_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(CVRXRModule).GetMethod(nameof(CVRXRModule.Reset), BindingFlags.Public | BindingFlags.Instance),
                new HarmonyLib.HarmonyMethod(typeof(ViveGesturesMovement).GetMethod(nameof(OnCVRXRModuleReset_Prefix), BindingFlags.NonPublic | BindingFlags.Static)),
                new HarmonyLib.HarmonyMethod(typeof(ViveGesturesMovement).GetMethod(nameof(OnCVRXRModuleReset_Postfix), BindingFlags.NonPublic | BindingFlags.Static))
            );
        }

        static void OnViveGesturesUpdate_Postfix(ref CVRXRModule __instance)
        {
            try
            {
                float l_mag = ((!__instance.HasEmoteOverride) ? __instance.Primary2DAxis : __instance.EmoteOverride).magnitude;
                if(__instance.ViveDirectionPressed && (l_mag >= CVRInputManager.VrViveGestureDeadZone))
                {
                    if(__instance.Grip > 0.5f)
                    {
                        __instance.GestureRaw = -1f;
                        __instance.Gesture = -1f;
                    }
                    else
                    {
                        __instance.GestureRaw = __instance.Trigger;
                        __instance.Gesture = __instance.Trigger;
                    }
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnCVRXRModuleReset_Prefix(ref CVRXRModule __instance, out bool __state)
        {
            __state = __instance.ViveDirectionPressed;
        }

        static void OnCVRXRModuleReset_Postfix(ref CVRXRModule __instance, bool __state)
        {
            if((__instance.Type == EXRControllerType.Vive) && CVRInputManager._moduleXR.ViveAdvancedControls)
                __instance.ViveDirectionPressed = __state;
        }
    }
}

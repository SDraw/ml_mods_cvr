using ABI_RC.Systems.InputManagement;
using ABI_RC.Systems.InputManagement.XR;
using System;
using System.Reflection;

namespace ml_vei
{
    public class ViveExtendedInput : MelonLoader.MelonMod
    {
        public override void OnInitializeMelon()
        {
            Settings.Init();

            HarmonyInstance.Patch(
                typeof(CVRXRModule).GetMethod("Update_Gestures_Vive", BindingFlags.NonPublic | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(ViveExtendedInput).GetMethod(nameof(OnViveGesturesUpdate_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
        }

        static void OnViveGesturesUpdate_Postfix(ref CVRXRModule __instance)
        {
            try
            {
                if(Settings.Gestures)
                {
                    float l_mag = ((!__instance.HasEmoteOverride) ? __instance.Primary2DAxis : __instance.EmoteOverride).magnitude;
                    if(__instance.ViveDirectionPressed && (l_mag >= CVRInputManager.VrViveGestureDeadZone))
                    {
                        if(Settings.GripTrigger)
                        {
                            switch(Settings.AxisPriority)
                            {
                                case Settings.PriorityAxis.Grip:
                                    __instance.GestureRaw = ((__instance.Grip > 0.5f) ? -1f : __instance.Trigger);
                                    break;

                                case Settings.PriorityAxis.Trigger:
                                    __instance.GestureRaw = (!UnityEngine.Mathf.Approximately(__instance.Trigger, 0f) ? __instance.Trigger : ((__instance.Grip > 0.5f) ? -1f : 0f));
                                    break;
                            }
                        }
                        else
                            __instance.GestureRaw = 0f;

                        __instance.Gesture = __instance.GestureRaw;
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

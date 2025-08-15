using ABI_RC.Core;
using System;
using System.Reflection;

namespace ml_gmf.Fixes
{
    static class AnimationOverrides
    {
        internal static void Init(HarmonyLib.Harmony p_instance)
        {
            p_instance.Patch(
                typeof(CVRAnimatorManager).GetMethod(nameof(CVRAnimatorManager.SetOverrideAnimation)),
                new HarmonyLib.HarmonyMethod(typeof(AnimationOverrides).GetMethod(nameof(OnOverride_Prefix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            p_instance.Patch(
                typeof(CVRAnimatorManager).GetMethod(nameof(CVRAnimatorManager.RestoreOverrideAnimation)),
                new HarmonyLib.HarmonyMethod(typeof(AnimationOverrides).GetMethod(nameof(OnOverride_Prefix), BindingFlags.Static | BindingFlags.NonPublic))
            );
        }

        static void OnOverride_Prefix(ref CVRAnimatorManager __instance)
        {
            try
            {
                if(__instance.animator != null)
                    __instance.animator.WriteDefaultValues();
            }
            catch(Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }
    }
}

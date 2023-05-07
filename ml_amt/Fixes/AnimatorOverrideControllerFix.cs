using ABI_RC.Core;
using System;
using System.Reflection;

namespace ml_amt.Fixes
{
    static class AnimatorOverrideControllerFix
    {
        internal static void Init(HarmonyLib.Harmony p_instance)
        {
            // AAS overriding fix
            p_instance.Patch(
                typeof(CVRAnimatorManager).GetMethod(nameof(CVRAnimatorManager.SetOverrideAnimation)),
                new HarmonyLib.HarmonyMethod(typeof(AnimatorOverrideControllerFix).GetMethod(nameof(OnOverride_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                new HarmonyLib.HarmonyMethod(typeof(AnimatorOverrideControllerFix).GetMethod(nameof(OnOverride_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            p_instance.Patch(
                typeof(CVRAnimatorManager).GetMethod(nameof(CVRAnimatorManager.RestoreOverrideAnimation)),
                new HarmonyLib.HarmonyMethod(typeof(AnimatorOverrideControllerFix).GetMethod(nameof(OnOverride_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                new HarmonyLib.HarmonyMethod(typeof(AnimatorOverrideControllerFix).GetMethod(nameof(OnOverride_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
        }

        // AnimatorOverrideController runtime animation replacement fix
        static void OnOverride_Prefix(ref CVRAnimatorManager __instance, out AnimatorAnalyzer __state)
        {
            __state = new AnimatorAnalyzer();
            try
            {
                if(Settings.OverrideFix && (__instance.animator != null))
                {
                    __state.AnalyzeFrom(__instance.animator);
                    if(__state.IsEnabled())
                        __instance.animator.enabled = false;
                    __instance.animator.WriteDefaultValues();
                }
            }
            catch(Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }
        static void OnOverride_Postfix(ref CVRAnimatorManager __instance, AnimatorAnalyzer __state)
        {
            try
            {
                if(Settings.OverrideFix && (__instance.animator != null))
                {
                    __state.ApplyTo(__instance.animator);
                    if(__state.IsEnabled())
                        __instance.animator.Update(0f);
                }
            }
            catch(Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }
    }
}

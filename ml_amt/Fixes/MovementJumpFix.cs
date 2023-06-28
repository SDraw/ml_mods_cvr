using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.MovementSystem;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace ml_amt.Fixes
{
    static class MovementJumpFix
    {
        static FieldInfo ms_avatarHeight = typeof(PlayerSetup).GetField("_avatarHeight", BindingFlags.NonPublic | BindingFlags.Instance);

        static float ms_playerHeight = 1f;

        internal static void Init(HarmonyLib.Harmony p_instance)
        {
            p_instance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetupAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(MovementJumpFix).GetMethod(nameof(OnSetupAvatar_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            p_instance.Patch(
                typeof(CVRWorld).GetMethod("SetupWorldRules", BindingFlags.NonPublic | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(MovementJumpFix).GetMethod(nameof(OnWorldRulesSetup_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            p_instance.Patch(
                typeof(PlayerSetup).GetMethod("SetupIKScaling", BindingFlags.NonPublic | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(MovementJumpFix).GetMethod(nameof(OnSetupIKScaling_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );

            Settings.ScaledJumpChange += OnScaledJumpChange;
            MelonLoader.MelonCoroutines.Start(WaitForGameSettings());
        }

        static IEnumerator WaitForGameSettings()
        {
            while(MetaPort.Instance == null)
                yield return null;
            while(MetaPort.Instance.settings == null)
                yield return null;

            ms_playerHeight = MetaPort.Instance.settings.GetSettingInt("GeneralPlayerHeight") * 0.01f;
            MetaPort.Instance.settings.settingIntChanged.AddListener(OnGameSettingIntChange);
        }

        // Patches
        static void OnSetupAvatar_Postfix()
        {
            try
            {
                SetScaledJump(Settings.ScaledJump);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
        static void OnWorldRulesSetup_Postfix()
        {
            try
            {
                SetScaledJump(Settings.ScaledJump);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnSetupIKScaling_Postfix()
        {
            try
            {
                SetScaledJump(Settings.ScaledJump);
            }
            catch(Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }

        // Mod settings
        static void OnScaledJumpChange(bool p_state)
        {
            SetScaledJump(p_state);
        }

        // Game settings
        static void OnGameSettingIntChange(string p_name, int p_value)
        {
            if(p_name == "GeneralPlayerHeight")
            {
                ms_playerHeight = p_value * 0.01f;
            }
        }

        // Arbitrary
        static void SetScaledJump(bool p_state)
        {
            if(Utils.IsWorldSafe())
            {
                if(p_state)
                    MovementSystem.Instance.jumpHeight = Mathf.Clamp(Utils.GetWorldJumpHeight() * ((float)ms_avatarHeight.GetValue(PlayerSetup.Instance) / ms_playerHeight), float.MinValue, Utils.GetWorldMovementLimit());
                else
                    MovementSystem.Instance.jumpHeight = Utils.GetWorldJumpHeight();
            }
        }
    }
}

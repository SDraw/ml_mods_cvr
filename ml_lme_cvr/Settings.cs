using ABI_RC.Core.Savior;
using System;
using UnityEngine;

namespace ml_lme_cvr
{
    static class Settings
    {
        public static readonly string[] ms_defaultSettings =
        {
            "InteractionLeapMotionTracking",
            "InteractionLeapMotionTrackingDesktopX",
            "InteractionLeapMotionTrackingDesktopY",
            "InteractionLeapMotionTrackingDesktopZ",
            "InteractionLeapMotionTrackingFingersOnly",
            "InteractionLeapMotionTrackingModel",
            "InteractionLeapMotionTrackingHmd",
            "InteractionLeapMotionTrackingAngle",
            "InteractionLeapMotionTrackingHead",
            "InteractionLeapMotionTrackingHeadX",
            "InteractionLeapMotionTrackingHeadY",
            "InteractionLeapMotionTrackingHeadZ"
        };

        static bool ms_enabled = false;
        static Vector3 ms_desktopOffset = new Vector3(0f, -0.45f, 0.3f);
        static bool ms_fingersOnly = false;
        static bool ms_modelVisibility = false;
        static bool ms_hmdMode = false;
        static float ms_rootAngle = 0f;
        static bool ms_headAttach = false;
        static Vector3 ms_headOffset = new Vector3(0f, -0.3f, 0.15f);

        static bool ms_initialized = false;

        static public event Action EnabledChange;
        static public event Action DesktopOffsetChange;
        static public event Action FingersOnlyChange;
        static public event Action ModelVisibilityChange;
        static public event Action HmdModeChange;
        static public event Action RootAngleChange;
        static public event Action HeadAttachChange;
        static public event Action HeadOffsetChange;

        public static void Init(HarmonyLib.Harmony p_instance)
        {
            p_instance.Patch(
                typeof(CVRSettings).GetMethod(nameof(CVRSettings.LoadSerializedSettings)),
                new HarmonyLib.HarmonyMethod(typeof(Settings).GetMethod(nameof(BeforeSettingsLoad), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)),
                null
            );
        }

        static void BeforeSettingsLoad(ref CVRSettings __instance)
        {
            if(!ms_initialized && (__instance != null))
            {
                var l_settings = HarmonyLib.Traverse.Create(__instance)?.Field("_settings")?.GetValue<System.Collections.Generic.List<ABI_RC.Core.Savior.CVRSettingsValue>>();
                if(l_settings != null)
                {
                    l_settings.Add(new CVRSettingsBool(ms_defaultSettings[0], false));
                    l_settings.Add(new CVRSettingsInt(ms_defaultSettings[1], 0));
                    l_settings.Add(new CVRSettingsInt(ms_defaultSettings[2], -45));
                    l_settings.Add(new CVRSettingsInt(ms_defaultSettings[3], 30));
                    l_settings.Add(new CVRSettingsBool(ms_defaultSettings[4], false));
                    l_settings.Add(new CVRSettingsBool(ms_defaultSettings[5], false));
                    l_settings.Add(new CVRSettingsBool(ms_defaultSettings[6], false));
                    l_settings.Add(new CVRSettingsInt(ms_defaultSettings[7], 0));
                    l_settings.Add(new CVRSettingsBool(ms_defaultSettings[8], false));
                    l_settings.Add(new CVRSettingsInt(ms_defaultSettings[9], 0));
                    l_settings.Add(new CVRSettingsInt(ms_defaultSettings[10], 0));
                    l_settings.Add(new CVRSettingsInt(ms_defaultSettings[11], 0));
                }

                // Enable tracking
                __instance.settingBoolChanged.AddListener((name, value) =>
                {
                    if(name == ms_defaultSettings[0])
                    {
                        ms_enabled = value;
                        EnabledChange?.Invoke();
                    }
                });

                // Desktop offsets
                __instance.settingIntChanged.AddListener((name, value) =>
                {
                    for(int i = 1; i <= 3; i++)
                    {
                        if(name == ms_defaultSettings[i])
                        {
                            ms_desktopOffset = new Vector3(
                                MetaPort.Instance.settings.GetSettingInt(ms_defaultSettings[1]),
                                MetaPort.Instance.settings.GetSettingInt(ms_defaultSettings[2]),
                                MetaPort.Instance.settings.GetSettingInt(ms_defaultSettings[3])
                            ) * 0.01f;
                            DesktopOffsetChange?.Invoke();
                            break;
                        }
                    }
                });

                // Fingers tracking only
                __instance.settingBoolChanged.AddListener((name, value) =>
                {
                    if(name == ms_defaultSettings[4])
                    {
                        ms_fingersOnly = value;
                        FingersOnlyChange?.Invoke();
                    }
                });

                // Model visibility
                __instance.settingBoolChanged.AddListener((name, value) =>
                {
                    if(name == ms_defaultSettings[5])
                    {
                        ms_modelVisibility = value;
                        ModelVisibilityChange?.Invoke();
                    }
                });

                // HMD mode
                __instance.settingBoolChanged.AddListener((name, value) =>
                {
                    if(name == ms_defaultSettings[6])
                    {
                        ms_hmdMode = value;
                        HmdModeChange?.Invoke();
                    }
                });

                // Root angle
                __instance.settingIntChanged.AddListener((name, value) =>
                {
                    if(name == ms_defaultSettings[7])
                    {
                        ms_rootAngle = value;
                        RootAngleChange?.Invoke();
                    }
                });

                // Head attach
                __instance.settingBoolChanged.AddListener((name, value) =>
                {
                    if(name == ms_defaultSettings[8])
                    {
                        ms_headAttach = value;
                        HeadAttachChange?.Invoke();
                    }
                });

                // Head offset
                __instance.settingIntChanged.AddListener((name, value) =>
                {
                    for(int i = 9; i <= 11; i++)
                    {
                        if(name == ms_defaultSettings[i])
                        {
                            ms_headOffset = new Vector3(
                                MetaPort.Instance.settings.GetSettingInt(ms_defaultSettings[9]),
                                MetaPort.Instance.settings.GetSettingInt(ms_defaultSettings[10]),
                                MetaPort.Instance.settings.GetSettingInt(ms_defaultSettings[11])
                            ) * 0.01f;
                            HeadOffsetChange?.Invoke();
                            break;
                        }
                    }
                });

                ms_initialized = true;
            }
        }

        static public void Reload()
        {
            ms_enabled = MetaPort.Instance.settings.GetSettingsBool(ms_defaultSettings[0]);
            ms_desktopOffset = new Vector3(
                MetaPort.Instance.settings.GetSettingInt(ms_defaultSettings[1]),
                MetaPort.Instance.settings.GetSettingInt(ms_defaultSettings[2]),
                MetaPort.Instance.settings.GetSettingInt(ms_defaultSettings[3])
            ) * 0.01f;
            ms_fingersOnly = MetaPort.Instance.settings.GetSettingsBool(ms_defaultSettings[4]);
            ms_modelVisibility = MetaPort.Instance.settings.GetSettingsBool(ms_defaultSettings[5]);
            ms_hmdMode = MetaPort.Instance.settings.GetSettingsBool(ms_defaultSettings[6]);
            ms_rootAngle = MetaPort.Instance.settings.GetSettingInt(ms_defaultSettings[7]);
            ms_headAttach = MetaPort.Instance.settings.GetSettingsBool(ms_defaultSettings[8]);
            ms_headOffset = new Vector3(
                MetaPort.Instance.settings.GetSettingInt(ms_defaultSettings[9]),
                MetaPort.Instance.settings.GetSettingInt(ms_defaultSettings[10]),
                MetaPort.Instance.settings.GetSettingInt(ms_defaultSettings[11])
            ) * 0.01f;
        }

        public static bool Enabled
        {
            get => ms_enabled;
        }

        public static Vector3 DesktopOffset
        {
            get => ms_desktopOffset;
        }

        public static bool FingersOnly
        {
            get => ms_fingersOnly;
        }

        public static bool ModelVisibility
        {
            get => ms_modelVisibility;
        }

        public static bool HmdMode
        {
            get => ms_hmdMode;
        }

        public static float RootAngle
        {
            get => ms_rootAngle;
        }

        public static bool HeadAttach
        {
            get => ms_headAttach;
        }

        public static Vector3 HeadOffset
        {
            get => ms_headOffset;
        }
    }
}

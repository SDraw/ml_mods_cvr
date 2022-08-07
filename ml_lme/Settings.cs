using ABI_RC.Core.Savior;
using System;
using UnityEngine;

namespace ml_lme
{
    static class Settings
    {
        public enum LeapTrackingMode
        {
            Screentop = 0,
            Desktop,
            HMD
        }

        enum ModSetting
        {
            InteractionLeapMotionTracking,
            InteractionLeapMotionTrackingDesktopX,
            InteractionLeapMotionTrackingDesktopY,
            InteractionLeapMotionTrackingDesktopZ,
            InteractionLeapMotionTrackingFingersOnly,
            InteractionLeapMotionTrackingModel,
            InteractionLeapMotionTrackingMode,
            InteractionLeapMotionTrackingAngle,
            InteractionLeapMotionTrackingHead,
            InteractionLeapMotionTrackingHeadX,
            InteractionLeapMotionTrackingHeadY,
            InteractionLeapMotionTrackingHeadZ
        };

        static bool ms_enabled = false;
        static Vector3 ms_desktopOffset = new Vector3(0f, -0.45f, 0.3f);
        static bool ms_fingersOnly = false;
        static bool ms_modelVisibility = false;
        static LeapTrackingMode ms_trackingMode = LeapTrackingMode.Desktop;
        static float ms_rootAngle = 0f;
        static bool ms_headAttach = false;
        static Vector3 ms_headOffset = new Vector3(0f, -0.3f, 0.15f);

        static bool ms_initialized = false;

        static public event Action EnabledChange;
        static public event Action DesktopOffsetChange;
        static public event Action FingersOnlyChange;
        static public event Action ModelVisibilityChange;
        static public event Action TrackingModeChange;
        static public event Action RootAngleChange;
        static public event Action HeadAttachChange;
        static public event Action HeadOffsetChange;

        public static void Init(HarmonyLib.Harmony p_instance)
        {
            p_instance.Patch(
                typeof(CVRSettings).GetMethod(nameof(CVRSettings.LoadSerializedSettings)),
                new HarmonyLib.HarmonyMethod(typeof(Settings).GetMethod(nameof(LoadSerializedSettings_Prefix), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)),
                null
            );
        }

        static void LoadSerializedSettings_Prefix(ref CVRSettings __instance)
        {
            if(!ms_initialized && (__instance != null))
            {
                var l_settings = HarmonyLib.Traverse.Create(__instance)?.Field("_settings")?.GetValue<System.Collections.Generic.List<ABI_RC.Core.Savior.CVRSettingsValue>>();
                if(l_settings != null)
                {
                    l_settings.Add(new CVRSettingsBool(ModSetting.InteractionLeapMotionTracking.ToString(), false));
                    l_settings.Add(new CVRSettingsInt(ModSetting.InteractionLeapMotionTrackingDesktopX.ToString(), 0));
                    l_settings.Add(new CVRSettingsInt(ModSetting.InteractionLeapMotionTrackingDesktopY.ToString(), -45));
                    l_settings.Add(new CVRSettingsInt(ModSetting.InteractionLeapMotionTrackingDesktopZ.ToString(), 30));
                    l_settings.Add(new CVRSettingsBool(ModSetting.InteractionLeapMotionTrackingFingersOnly.ToString(), false));
                    l_settings.Add(new CVRSettingsBool(ModSetting.InteractionLeapMotionTrackingModel.ToString(), false));
                    l_settings.Add(new CVRSettingsInt(ModSetting.InteractionLeapMotionTrackingMode.ToString(), 1));
                    l_settings.Add(new CVRSettingsInt(ModSetting.InteractionLeapMotionTrackingAngle.ToString(), 0));
                    l_settings.Add(new CVRSettingsBool(ModSetting.InteractionLeapMotionTrackingHead.ToString(), false));
                    l_settings.Add(new CVRSettingsInt(ModSetting.InteractionLeapMotionTrackingHeadX.ToString(), 0));
                    l_settings.Add(new CVRSettingsInt(ModSetting.InteractionLeapMotionTrackingHeadY.ToString(), 0));
                    l_settings.Add(new CVRSettingsInt(ModSetting.InteractionLeapMotionTrackingHeadZ.ToString(), 0));
                }

                __instance.settingBoolChanged.AddListener((name, value) =>
                {
                    if(Enum.TryParse(name, out ModSetting l_setting))
                    {
                        switch(l_setting)
                        {
                            case ModSetting.InteractionLeapMotionTracking:
                            {
                                ms_enabled = value;
                                EnabledChange?.Invoke();
                            }
                            break;

                            case ModSetting.InteractionLeapMotionTrackingFingersOnly:
                            {
                                ms_fingersOnly = value;
                                FingersOnlyChange?.Invoke();
                            }
                            break;

                            case ModSetting.InteractionLeapMotionTrackingModel:
                            {
                                ms_modelVisibility = value;
                                ModelVisibilityChange?.Invoke();
                            }
                            break;

                            case ModSetting.InteractionLeapMotionTrackingHead:
                            {
                                ms_headAttach = value;
                                HeadAttachChange?.Invoke();
                            }
                            break;
                        }
                    }
                });

                __instance.settingIntChanged.AddListener((name, value) =>
                {
                    if(Enum.TryParse(name, out ModSetting l_setting))
                    {
                        switch(l_setting)
                        {
                            case ModSetting.InteractionLeapMotionTrackingDesktopX:
                            case ModSetting.InteractionLeapMotionTrackingDesktopY:
                            case ModSetting.InteractionLeapMotionTrackingDesktopZ:
                            {
                                ms_desktopOffset = new Vector3(
                                    MetaPort.Instance.settings.GetSettingInt(ModSetting.InteractionLeapMotionTrackingDesktopX.ToString()),
                                    MetaPort.Instance.settings.GetSettingInt(ModSetting.InteractionLeapMotionTrackingDesktopY.ToString()),
                                    MetaPort.Instance.settings.GetSettingInt(ModSetting.InteractionLeapMotionTrackingDesktopZ.ToString())
                                ) * 0.01f;
                                DesktopOffsetChange?.Invoke();
                            }
                            break;

                            case ModSetting.InteractionLeapMotionTrackingMode:
                            {
                                ms_trackingMode = (LeapTrackingMode)value;
                                TrackingModeChange?.Invoke();
                            }
                            break;

                            case ModSetting.InteractionLeapMotionTrackingAngle:
                            {
                                ms_rootAngle = value;
                                RootAngleChange?.Invoke();
                            }
                            break;

                            case ModSetting.InteractionLeapMotionTrackingHeadX:
                            case ModSetting.InteractionLeapMotionTrackingHeadY:
                            case ModSetting.InteractionLeapMotionTrackingHeadZ:
                            {
                                ms_headOffset = new Vector3(
                                    MetaPort.Instance.settings.GetSettingInt(ModSetting.InteractionLeapMotionTrackingHeadX.ToString()),
                                    MetaPort.Instance.settings.GetSettingInt(ModSetting.InteractionLeapMotionTrackingHeadY.ToString()),
                                    MetaPort.Instance.settings.GetSettingInt(ModSetting.InteractionLeapMotionTrackingHeadZ.ToString())
                                ) * 0.01f;
                                HeadOffsetChange?.Invoke();
                            }
                            break;
                        }
                    }
                });

                ms_initialized = true;
            }
        }

        static public void Reload()
        {
            ms_enabled = MetaPort.Instance.settings.GetSettingsBool(ModSetting.InteractionLeapMotionTracking.ToString());
            ms_desktopOffset = new Vector3(
                MetaPort.Instance.settings.GetSettingInt(ModSetting.InteractionLeapMotionTrackingDesktopX.ToString()),
                MetaPort.Instance.settings.GetSettingInt(ModSetting.InteractionLeapMotionTrackingDesktopY.ToString()),
                MetaPort.Instance.settings.GetSettingInt(ModSetting.InteractionLeapMotionTrackingDesktopZ.ToString())
            ) * 0.01f;
            ms_fingersOnly = MetaPort.Instance.settings.GetSettingsBool(ModSetting.InteractionLeapMotionTrackingFingersOnly.ToString());
            ms_modelVisibility = MetaPort.Instance.settings.GetSettingsBool(ModSetting.InteractionLeapMotionTrackingModel.ToString());
            ms_trackingMode = (LeapTrackingMode)MetaPort.Instance.settings.GetSettingInt(ModSetting.InteractionLeapMotionTrackingMode.ToString());
            ms_rootAngle = MetaPort.Instance.settings.GetSettingInt(ModSetting.InteractionLeapMotionTrackingAngle.ToString());
            ms_headAttach = MetaPort.Instance.settings.GetSettingsBool(ModSetting.InteractionLeapMotionTrackingHead.ToString());
            ms_headOffset = new Vector3(
                MetaPort.Instance.settings.GetSettingInt(ModSetting.InteractionLeapMotionTrackingHeadX.ToString()),
                MetaPort.Instance.settings.GetSettingInt(ModSetting.InteractionLeapMotionTrackingHeadY.ToString()),
                MetaPort.Instance.settings.GetSettingInt(ModSetting.InteractionLeapMotionTrackingHeadZ.ToString())
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

        public static LeapTrackingMode TrackingMode
        {
            get => ms_trackingMode;
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

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
            "InteractionLeapMotionTrackingFingersOnly"
        };

        static bool ms_enabled = false;
        static float ms_desktopOffsetX = 0f;
        static float ms_desktopOffsetY = -0.45f;
        static float ms_desktopOffsetZ = 0.3f;
        static bool ms_fingersOnly = false;

        static bool ms_initialized = false;

        static public event System.Action EnabledChange;
        static public event System.Action DesktopOffsetChange;
        static public event System.Action FingersOnlyChange;

        public static void Init(HarmonyLib.Harmony p_instance)
        {
            p_instance.Patch(
                typeof(ABI_RC.Core.Savior.CVRSettings).GetMethod(nameof(ABI_RC.Core.Savior.CVRSettings.LoadSerializedSettings)),
                new HarmonyLib.HarmonyMethod(typeof(Settings).GetMethod(nameof(BeforeSettingsLoad), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)),
                null
            );
        }

        static void BeforeSettingsLoad(ref ABI_RC.Core.Savior.CVRSettings __instance)
        {
            if(!ms_initialized && __instance != null)
            {
                var l_settings = HarmonyLib.Traverse.Create(__instance)?.Field("_settings")?.GetValue<System.Collections.Generic.List<ABI_RC.Core.Savior.CVRSettingsValue>>();
                if(l_settings != null)
                {
                    l_settings.Add(new ABI_RC.Core.Savior.CVRSettingsBool(ms_defaultSettings[0], false));
                    l_settings.Add(new ABI_RC.Core.Savior.CVRSettingsInt(ms_defaultSettings[1], 0));
                    l_settings.Add(new ABI_RC.Core.Savior.CVRSettingsInt(ms_defaultSettings[2], -45));
                    l_settings.Add(new ABI_RC.Core.Savior.CVRSettingsInt(ms_defaultSettings[3], 30));
                    l_settings.Add(new ABI_RC.Core.Savior.CVRSettingsBool(ms_defaultSettings[4], false));
                }

                // Changes events
                __instance.settingBoolChanged.AddListener((name, value) =>
                {
                    if(name == ms_defaultSettings[0])
                    {
                        Settings.Reload();
                        EnabledChange?.Invoke();
                    }
                });

                __instance.settingIntChanged.AddListener((name, value) =>
                {
                    for(int i=1; i <= 3; i++)
                    {
                        if(name == ms_defaultSettings[i])
                        {
                            Settings.Reload();
                            DesktopOffsetChange?.Invoke();
                            break;
                        }
                    }
                });

                __instance.settingBoolChanged.AddListener((name, value) =>
                {
                    if(name == ms_defaultSettings[4])
                    {
                        Settings.Reload();
                        FingersOnlyChange?.Invoke();
                    }
                });

                ms_initialized = true;
            }
        }

        static public void Reload()
        {
            ms_enabled = ABI_RC.Core.Savior.MetaPort.Instance.settings.GetSettingsBool(ms_defaultSettings[0]);
            ms_desktopOffsetX = ABI_RC.Core.Savior.MetaPort.Instance.settings.GetSettingInt(ms_defaultSettings[1]) * 0.01f;
            ms_desktopOffsetY = ABI_RC.Core.Savior.MetaPort.Instance.settings.GetSettingInt(ms_defaultSettings[2]) * 0.01f;
            ms_desktopOffsetZ = ABI_RC.Core.Savior.MetaPort.Instance.settings.GetSettingInt(ms_defaultSettings[3]) * 0.01f;
            ms_fingersOnly = ABI_RC.Core.Savior.MetaPort.Instance.settings.GetSettingsBool(ms_defaultSettings[4]);
        }

        public static bool Enabled
        {
            get => ms_enabled;
        }

        public static float DesktopOffsetX
        {
            get => ms_desktopOffsetX;
        }
        public static float DesktopOffsetY
        {
            get => ms_desktopOffsetY;
        }
        public static float DesktopOffsetZ
        {
            get => ms_desktopOffsetZ;
        }

        public static bool FingersOnly
        {
            get => ms_fingersOnly;
        }
    }
}

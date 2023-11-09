using ABI_RC.Core.InteractionSystem;
using System;
using System.Collections.Generic;

namespace ml_dht
{
    static class Settings
    {
        enum ModSetting
        {
            Enabled = 0,
            HeadTracking,
            EyeTracking,
            Blinking,
            Mirrored,
            Smoothing,
            FaceOverride
        }

        public static bool Enabled { get; private set; } = false;
        public static bool HeadTracking { get; private set; } = true;
        public static bool EyeTracking { get; private set; } = true;
        public static bool Blinking { get; private set; } = true;
        public static bool Mirrored { get; private set; } = false;
        public static float Smoothing { get; private set; } = 0.5f;
        public static bool FaceOverride { get; private set; } = true;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        static public event Action<bool> EnabledChange;
        static public event Action<bool> HeadTrackingChange;
        static public event Action<bool> EyeTrackingChange;
        static public event Action<bool> BlinkingChange;
        static public event Action<bool> MirroredChange;
        static public event Action<float> SmoothingChange;
        static public event Action<bool> FaceOverrideChange;

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("DHT");

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.Enabled.ToString(), Enabled),
                ms_category.CreateEntry(ModSetting.HeadTracking.ToString(), HeadTracking),
                ms_category.CreateEntry(ModSetting.EyeTracking.ToString(), EyeTracking),
                ms_category.CreateEntry(ModSetting.Blinking.ToString(), Blinking),
                ms_category.CreateEntry(ModSetting.Mirrored.ToString(), Mirrored),
                ms_category.CreateEntry(ModSetting.Smoothing.ToString(), (int)(Smoothing * 50f)),
                ms_category.CreateEntry(ModSetting.FaceOverride.ToString(), FaceOverride)
            };

            Load();

            MelonLoader.MelonCoroutines.Start(WaitMainMenuUi());
        }

        static System.Collections.IEnumerator WaitMainMenuUi()
        {
            while(ViewManager.Instance == null)
                yield return null;
            while(ViewManager.Instance.gameMenuView == null)
                yield return null;
            while(ViewManager.Instance.gameMenuView.Listener == null)
                yield return null;

            ViewManager.Instance.gameMenuView.Listener.ReadyForBindings += () =>
            {
                ViewManager.Instance.gameMenuView.View.BindCall("MelonMod_DHT_Call_InpSlider", new Action<string, string>(OnSliderUpdate));
                ViewManager.Instance.gameMenuView.View.BindCall("MelonMod_DHT_Call_InpToggle", new Action<string, string>(OnToggleUpdate));
            };
            ViewManager.Instance.gameMenuView.Listener.FinishLoad += (_) =>
            {
                ViewManager.Instance.gameMenuView.View.ExecuteScript(Scripts.GetEmbeddedScript("menu.js"));
                foreach(var l_entry in ms_entries)
                    ViewManager.Instance.gameMenuView.View.TriggerEvent("updateModSettingDHT", l_entry.DisplayName, l_entry.GetValueAsString());
            };
        }

        static void Load()
        {
            Enabled = (bool)ms_entries[(int)ModSetting.Enabled].BoxedValue;
            HeadTracking = (bool)ms_entries[(int)ModSetting.HeadTracking].BoxedValue;
            EyeTracking = (bool)ms_entries[(int)ModSetting.EyeTracking].BoxedValue;
            Blinking = (bool)ms_entries[(int)ModSetting.Blinking].BoxedValue;
            Mirrored = (bool)ms_entries[(int)ModSetting.Mirrored].BoxedValue;
            Smoothing = ((int)ms_entries[(int)ModSetting.Smoothing].BoxedValue) * 0.01f;
            FaceOverride = (bool)ms_entries[(int)ModSetting.FaceOverride].BoxedValue;
        }

        static void OnSliderUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.Smoothing:
                    {
                        Smoothing = int.Parse(p_value) * 0.01f;
                        SmoothingChange?.Invoke(Smoothing);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = int.Parse(p_value);
            }
        }

        static void OnToggleUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.Enabled:
                    {
                        Enabled = bool.Parse(p_value);
                        EnabledChange?.Invoke(Enabled);
                    }
                    break;

                    case ModSetting.HeadTracking:
                    {
                        HeadTracking = bool.Parse(p_value);
                        HeadTrackingChange?.Invoke(HeadTracking);
                    }
                    break;

                    case ModSetting.EyeTracking:
                    {
                        EyeTracking = bool.Parse(p_value);
                        EyeTrackingChange?.Invoke(EyeTracking);
                    }
                    break;

                    case ModSetting.Blinking:
                    {
                        Blinking = bool.Parse(p_value);
                        BlinkingChange?.Invoke(Blinking);
                    }
                    break;

                    case ModSetting.Mirrored:
                    {
                        Mirrored = bool.Parse(p_value);
                        MirroredChange?.Invoke(Mirrored);
                    }
                    break;

                    case ModSetting.FaceOverride:
                    {
                        FaceOverride = bool.Parse(p_value);
                        FaceOverrideChange?.Invoke(FaceOverride);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = bool.Parse(p_value);
            }
        }
    }
}

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
            FaceTracking,
            Blinking,
            Mirrored,
            Smoothing,
        }

        public static bool Enabled { get; private set; } = false;
        public static bool HeadTracking { get; private set; } = true;
        public static bool EyeTracking { get; private set; } = true;
        public static bool FaceTracking { get; private set; } = true;
        public static bool Blinking { get; private set; } = true;
        public static bool Mirrored { get; private set; } = false;
        public static float Smoothing { get; private set; } = 0.5f;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        public static event Action<bool> EnabledChange;
        public static event Action<bool> HeadTrackingChange;
        public static event Action<bool> EyeTrackingChange;
        public static event Action<bool> FaceTrackingChange;
        public static event Action<bool> BlinkingChange;
        public static event Action<bool> MirroredChange;
        public static event Action<float> SmoothingChange;

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("DHT");

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.Enabled.ToString(), Enabled),
                ms_category.CreateEntry(ModSetting.HeadTracking.ToString(), HeadTracking),
                ms_category.CreateEntry(ModSetting.EyeTracking.ToString(), EyeTracking),
                ms_category.CreateEntry(ModSetting.FaceTracking.ToString(), FaceTracking),
                ms_category.CreateEntry(ModSetting.Blinking.ToString(), Blinking),
                ms_category.CreateEntry(ModSetting.Mirrored.ToString(), Mirrored),
                ms_category.CreateEntry(ModSetting.Smoothing.ToString(), (int)(Smoothing * 50f)),
            };

            Enabled = (bool)ms_entries[(int)ModSetting.Enabled].BoxedValue;
            HeadTracking = (bool)ms_entries[(int)ModSetting.HeadTracking].BoxedValue;
            EyeTracking = (bool)ms_entries[(int)ModSetting.EyeTracking].BoxedValue;
            FaceTracking = (bool)ms_entries[(int)ModSetting.FaceTracking].BoxedValue;
            Blinking = (bool)ms_entries[(int)ModSetting.Blinking].BoxedValue;
            Mirrored = (bool)ms_entries[(int)ModSetting.Mirrored].BoxedValue;
            Smoothing = ((int)ms_entries[(int)ModSetting.Smoothing].BoxedValue) * 0.01f;

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
                ViewManager.Instance.gameMenuView.View.BindCall("OnToggleUpdate_" + ms_category.Identifier, new Action<string, string>(OnToggleUpdate));
                ViewManager.Instance.gameMenuView.View.BindCall("OnSliderUpdate_" + ms_category.Identifier, new Action<string, string>(OnSliderUpdate));
            };
            ViewManager.Instance.gameMenuView.Listener.FinishLoad += (_) =>
            {
                ViewManager.Instance.gameMenuView.View.ExecuteScript(ResourcesHandler.GetEmbeddedResource("mods_extension.js"));
                ViewManager.Instance.gameMenuView.View.ExecuteScript(ResourcesHandler.GetEmbeddedResource("mod_menu.js"));
                foreach(var l_entry in ms_entries)
                    ViewManager.Instance.gameMenuView.View.TriggerEvent("updateModSetting", ms_category.Identifier, l_entry.DisplayName, l_entry.GetValueAsString());
            };
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

                    case ModSetting.FaceTracking:
                    {
                        FaceTracking = bool.Parse(p_value);
                        FaceTrackingChange?.Invoke(FaceTracking);
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
                }

                ms_entries[(int)l_setting].BoxedValue = bool.Parse(p_value);
            }
        }
    }
}

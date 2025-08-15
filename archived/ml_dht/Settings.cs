using ABI_RC.Core.InteractionSystem;
using System;
using System.Collections.Generic;

namespace ml_dht
{
    static class Settings
    {
        internal class SettingEvent<T>
        {
            event Action<T> m_action;
            public void AddListener(Action<T> p_listener) => m_action += p_listener;
            public void RemoveListener(Action<T> p_listener) => m_action -= p_listener;
            public void Invoke(T p_value) => m_action?.Invoke(p_value);
        }

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

        public static readonly SettingEvent<bool> OnEnabledChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnHeadTrackingChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnEyeTrackingChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnFaceTrackingChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnBlinkingChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnMirroredChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<float> OnSmoothingChanged = new SettingEvent<float>();

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
            while(ViewManager.Instance.cohtmlView == null)
                yield return null;
            while(ViewManager.Instance.cohtmlView.Listener == null)
                yield return null;

            ViewManager.Instance.cohtmlView.Listener.ReadyForBindings += () =>
            {
                ViewManager.Instance.cohtmlView.View.BindCall("OnToggleUpdate_" + ms_category.Identifier, new Action<string, string>(OnToggleUpdate));
                ViewManager.Instance.cohtmlView.View.BindCall("OnSliderUpdate_" + ms_category.Identifier, new Action<string, string>(OnSliderUpdate));
            };
            ViewManager.Instance.cohtmlView.Listener.FinishLoad += (_) =>
            {
                ViewManager.Instance.cohtmlView.View.ExecuteScript(ResourcesHandler.GetEmbeddedResource("mods_extension.js"));
                ViewManager.Instance.cohtmlView.View.ExecuteScript(ResourcesHandler.GetEmbeddedResource("mod_menu.js"));
                MelonLoader.MelonCoroutines.Start(UpdateMenuSettings());
            };
        }

        static System.Collections.IEnumerator UpdateMenuSettings()
        {
            while(!ViewManager.Instance.IsReady || !ViewManager.Instance.IsViewShown)
                yield return null;

            foreach(var l_entry in ms_entries)
                ViewManager.Instance.cohtmlView.View.TriggerEvent("updateModSetting", ms_category.Identifier, l_entry.DisplayName, l_entry.GetValueAsString());
        }

        static void OnSliderUpdate(string p_name, string p_value)
        {
            try
            {
                if(Enum.TryParse(p_name, out ModSetting l_setting) && int.TryParse(p_value, out int l_value))
                {
                    switch(l_setting)
                    {
                        case ModSetting.Smoothing:
                        {
                            Smoothing = l_value * 0.01f;
                            OnSmoothingChanged.Invoke(Smoothing);
                        }
                        break;
                    }

                    ms_entries[(int)l_setting].BoxedValue = l_value;
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnToggleUpdate(string p_name, string p_value)
        {
            try
            {
                if(Enum.TryParse(p_name, out ModSetting l_setting) && bool.TryParse(p_value, out bool l_value))
                {
                    switch(l_setting)
                    {
                        case ModSetting.Enabled:
                        {
                            Enabled = l_value;
                            OnEnabledChanged.Invoke(Enabled);
                        }
                        break;

                        case ModSetting.HeadTracking:
                        {
                            HeadTracking = l_value;
                            OnHeadTrackingChanged.Invoke(HeadTracking);
                        }
                        break;

                        case ModSetting.EyeTracking:
                        {
                            EyeTracking = l_value;
                            OnEyeTrackingChanged.Invoke(EyeTracking);
                        }
                        break;

                        case ModSetting.FaceTracking:
                        {
                            FaceTracking = l_value;
                            OnFaceTrackingChanged.Invoke(FaceTracking);
                        }
                        break;

                        case ModSetting.Blinking:
                        {
                            Blinking = l_value;
                            OnBlinkingChanged.Invoke(Blinking);
                        }
                        break;

                        case ModSetting.Mirrored:
                        {
                            Mirrored = l_value;
                            OnMirroredChanged.Invoke(Mirrored);
                        }
                        break;
                    }

                    ms_entries[(int)l_setting].BoxedValue = l_value;
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

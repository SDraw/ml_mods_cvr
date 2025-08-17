using ABI_RC.Core.InteractionSystem;
using System;
using System.Collections.Generic;

namespace ml_vet
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
            Smoothing,
            Debug
        }

        public static bool Enabled { get; private set; } = true;
        public static float Smoothing { get; private set; } = 0f;
        public static bool Debug { get; private set; } = false;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        public static readonly SettingEvent<bool> OnEnabledChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<float> OnSmoothingChanged = new SettingEvent<float>();
        public static readonly SettingEvent<bool> OnDebugChanged = new SettingEvent<bool>();

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("VET", null, true);

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.Enabled.ToString(), Enabled),
                ms_category.CreateEntry(ModSetting.Smoothing.ToString(), (int)(Smoothing * 100f)),
                ms_category.CreateEntry(ModSetting.Debug.ToString(), Debug),
            };

            Enabled = (bool)ms_entries[(int)ModSetting.Enabled].BoxedValue;
            Smoothing = ((int)ms_entries[(int)ModSetting.Smoothing].BoxedValue) * 0.01f;
            Debug = (bool)ms_entries[(int)ModSetting.Debug].BoxedValue;

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

                        case ModSetting.Debug:
                        {
                            Debug = l_value;
                            OnDebugChanged.Invoke(Debug);
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
    }
}

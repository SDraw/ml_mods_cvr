using ABI_RC.Core.InteractionSystem;
using System;
using System.Collections.Generic;

namespace ml_vpc
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

        public enum CookieMode
        {
            File = 0,
            BrowserFirefox,
            BrowserBrave,
            BrowserChrome, // This one might not work
            BrowserChromium,
            BrowserEdge,
            BrowserOpera,
            BrowserSafari,
            BrowserVivaldi,
            BrowserWhale,
        }

        private enum ModSetting
        {
            Enabled = 0,
            Mode,
        }

        public static bool Enabled { get; private set; } = true;
        public static CookieMode Mode { get; private set; } = CookieMode.File;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        public static readonly SettingEvent<bool> OnEnabledChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<CookieMode> OnModeChanged = new SettingEvent<CookieMode>();

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("VPC", null, true);

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(nameof(ModSetting.Enabled), Enabled),
                ms_category.CreateEntry(nameof(ModSetting.Mode), (int)CookieMode.File),
            };

            Enabled = (bool)ms_entries[(int)ModSetting.Enabled].BoxedValue;
            Mode = (CookieMode)ms_entries[(int)ModSetting.Mode].BoxedValue;

            MelonLoader.MelonCoroutines.Start(WaitMainMenuUi());
        }

        private static System.Collections.IEnumerator WaitMainMenuUi()
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
                ViewManager.Instance.cohtmlView.View.BindCall("OnDropdownUpdate_" + ms_category.Identifier, new Action<string, string>(OnDropdownUpdate));

            };
            ViewManager.Instance.cohtmlView.Listener.FinishLoad += (_) =>
            {
                ViewManager.Instance.cohtmlView.View.ExecuteScript(ResourcesHandler.GetEmbeddedResource("mods_extension.js"));
                ViewManager.Instance.cohtmlView.View.ExecuteScript(ResourcesHandler.GetEmbeddedResource("mod_menu.js"));
                MelonLoader.MelonCoroutines.Start(UpdateMenuSettings());
            };
        }

        private static System.Collections.IEnumerator UpdateMenuSettings()
        {
            while(!ViewManager.Instance.IsReady || !ViewManager.Instance.IsViewShown)
                yield return null;

            foreach(var l_entry in ms_entries)
                ViewManager.Instance.cohtmlView.View.TriggerEvent("updateModSetting", ms_category.Identifier, l_entry.DisplayName, l_entry.GetValueAsString());
        }

        private static void OnToggleUpdate(string p_name, string p_value)
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
                    }

                    ms_entries[(int)l_setting].BoxedValue = l_value;
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        private static void OnDropdownUpdate(string p_name, string p_value)
        {
            try
            {
                if(Enum.TryParse(p_name, out ModSetting l_setting) && int.TryParse(p_value, out int l_value))
                {
                    switch(l_setting)
                    {
                        case ModSetting.Mode:
                        {
                            Mode = (CookieMode)l_value;
                            OnModeChanged.Invoke(Mode);
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

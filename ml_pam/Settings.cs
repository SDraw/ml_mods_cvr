using ABI_RC.Core.InteractionSystem;
using cohtml;
using System;
using System.Collections.Generic;

namespace ml_pam
{
    static class Settings
    {
        public enum ModSetting
        {
            Enabled = 0,
            GrabOffset
        }

        static bool ms_enabled = true;
        static float ms_grabOffset = 0.25f;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        static public event Action<bool> EnabledChange;
        static public event Action<float> GrabOffsetChange;

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("PAM");

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.Enabled.ToString(), ms_enabled),
                ms_category.CreateEntry(ModSetting.GrabOffset.ToString(), 25),
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
                ViewManager.Instance.gameMenuView.View.BindCall("MelonMod_PAM_Call_InpToggle", new Action<string, string>(OnToggleUpdate));
                ViewManager.Instance.gameMenuView.View.BindCall("MelonMod_PAM_Call_InpSlider", new Action<string, string>(OnSliderUpdate));
            };
            ViewManager.Instance.gameMenuView.Listener.FinishLoad += (_) =>
            {
                ViewManager.Instance.gameMenuView.View.ExecuteScript(Scripts.GetEmbeddedScript("menu.js"));
                foreach(var l_entry in ms_entries)
                    ViewManager.Instance.gameMenuView.View.TriggerEvent("updateModSettingPAM", l_entry.DisplayName, l_entry.GetValueAsString());
            };
        }

        static void Load()
        {
            ms_enabled = (bool)ms_entries[(int)ModSetting.Enabled].BoxedValue;
            ms_grabOffset = (int)ms_entries[(int)ModSetting.GrabOffset].BoxedValue * 0.01f;
        }

        static void OnToggleUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.Enabled:
                    {
                        ms_enabled = bool.Parse(p_value);
                        EnabledChange?.Invoke(ms_enabled);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = bool.Parse(p_value);
            }
        }

        static void OnSliderUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.GrabOffset:
                    {
                        ms_grabOffset = int.Parse(p_value) * 0.01f;
                        GrabOffsetChange?.Invoke(ms_grabOffset);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = int.Parse(p_value);
            }
        }

        public static bool Enabled
        {
            get => ms_enabled;
        }
        public static float GrabOffset
        {
            get => ms_grabOffset;
        }
    }
}

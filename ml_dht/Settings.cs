using ABI_RC.Core.InteractionSystem;
using cohtml;
using System;
using System.Collections.Generic;

namespace ml_dht
{
    static class Settings
    {
        enum ModSetting
        {
            Enabled = 0,
            Mirrored,
            Smoothing
        }
        
        static bool ms_enabled = false;
        static bool ms_mirrored = false;
        static float ms_smoothing = 0.5f;
        
        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;
        
        static public event Action<bool> EnabledChange;
        static public event Action<bool> MirroredChange;
        static public event Action<float> SmoothingChange;
        
        public static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("DHT");
            
            ms_entries = new List<MelonLoader.MelonPreferences_Entry>();
            ms_entries.Add(ms_category.CreateEntry(ModSetting.Enabled.ToString(), false));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.Mirrored.ToString(), false));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.Smoothing.ToString(), 50));
            
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
            ms_enabled = (bool)ms_entries[(int)ModSetting.Enabled].BoxedValue;
            ms_mirrored = (bool)ms_entries[(int)ModSetting.Mirrored].BoxedValue;
            ms_smoothing = ((int)ms_entries[(int)ModSetting.Smoothing].BoxedValue) * 0.01f;
        }
        
        static void OnSliderUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.Smoothing:
                    {
                        ms_smoothing = int.Parse(p_value) * 0.01f;
                        SmoothingChange?.Invoke(ms_smoothing);
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
                        ms_enabled = bool.Parse(p_value);
                        EnabledChange?.Invoke(ms_enabled);
                    }
                    break;

                    case ModSetting.Mirrored:
                    {
                        ms_mirrored = bool.Parse(p_value);
                        MirroredChange?.Invoke(ms_mirrored);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = bool.Parse(p_value);
            }
        }
        
        public static bool Enabled
        {
            get => ms_enabled;
        }
        public static bool Mirrored
        {
            get => ms_mirrored;
        }
        public static float Smoothing
        {
            get => ms_smoothing;
        }
    }
}

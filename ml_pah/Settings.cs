using System;
using System.Collections.Generic;
using UnityEngine;

namespace ml_pah
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

        public enum ModSetting
        {
            AvatarsLimit = 0,
            AutosaveTime
        }

        public static int AvatarsLimit { get; private set; } = 12;
        public static int AutosaveTime { get; private set; } = 15;

        public static readonly SettingEvent<int> OnAvatarsLimitChanged = new SettingEvent<int>();
        public static readonly SettingEvent<int> OnAutosaveTimeChanged = new SettingEvent<int>();

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("PAH", "Player Avatar History");

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.AvatarsLimit.ToString(), AvatarsLimit),
                ms_category.CreateEntry(ModSetting.AutosaveTime.ToString(), AutosaveTime)
            };

            AvatarsLimit = Mathf.Clamp((int)ms_entries[(int)ModSetting.AvatarsLimit].BoxedValue, 10, 100);
            AutosaveTime = Mathf.Clamp((int)ms_entries[(int)ModSetting.AutosaveTime].BoxedValue, 0, 60);
        }

        public static void SetSetting(ModSetting p_settings, object p_value)
        {
            try
            {
                switch(p_settings)
                {
                    // Booleans
                    case ModSetting.AvatarsLimit:
                    {
                        AvatarsLimit = (int)p_value;
                        OnAvatarsLimitChanged.Invoke(AvatarsLimit);
                    }
                    break;

                    case ModSetting.AutosaveTime:
                    {
                        AutosaveTime = (int)p_value;
                        OnAutosaveTimeChanged.Invoke(AutosaveTime);
                    }
                    break;
                }

                if(ms_entries != null)
                    ms_entries[(int)p_settings].BoxedValue = p_value;
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

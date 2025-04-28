using System;
using System.Collections.Generic;
using UnityEngine;

namespace ml_ppu
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
            Enabled = 0,
            FriendsOnly,
            VelocityMultiplier
        }

        public static bool Enabled { get; private set; } = true;
        public static bool FriendsOnly { get; private set; } = true;
        public static float VelocityMultiplier { get; private set; } = 1f;

        public static readonly SettingEvent<bool> OnEnabledChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnFriendsOnlyChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<float> OnVelocityMultiplierChanged = new SettingEvent<float>();

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("PPU", "Player Pick Up", true);

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.Enabled.ToString(), Enabled),
                ms_category.CreateEntry(ModSetting.FriendsOnly.ToString(), FriendsOnly),
                ms_category.CreateEntry(ModSetting.VelocityMultiplier.ToString(), VelocityMultiplier)
            };

            Enabled = (bool)ms_entries[(int)ModSetting.Enabled].BoxedValue;
            FriendsOnly = (bool)ms_entries[(int)ModSetting.FriendsOnly].BoxedValue;
            VelocityMultiplier = Mathf.Clamp((float)ms_entries[(int)ModSetting.VelocityMultiplier].BoxedValue, 0f, 50f);
        }

        public static void SetSetting(ModSetting p_settings, object p_value)
        {
            try
            {
                switch(p_settings)
                {
                    // Booleans
                    case ModSetting.Enabled:
                    {
                        Enabled = (bool)p_value;
                        OnEnabledChanged.Invoke(Enabled);
                    }
                    break;

                    case ModSetting.FriendsOnly:
                    {
                        FriendsOnly = (bool)p_value;
                        OnFriendsOnlyChanged.Invoke(FriendsOnly);
                    }
                    break;

                    // Floats
                    case ModSetting.VelocityMultiplier:
                    {
                        VelocityMultiplier = (float)p_value;
                        OnVelocityMultiplierChanged.Invoke(VelocityMultiplier);
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

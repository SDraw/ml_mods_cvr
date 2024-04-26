using ABI_RC.Core.InteractionSystem;
using System;
using System.Collections.Generic;

namespace ml_amt
{
    static class Settings
    {
        internal class SettingEvent<T>
        {
            event Action<T> m_action;
            public void AddHandler(Action<T> p_listener) => m_action += p_listener;
            public void RemoveHandler(Action<T> p_listener) => m_action -= p_listener;
            public void Invoke(T p_value) => m_action?.Invoke(p_value);
        }

        enum ModSetting
        {
            CrouchLimit,
            ProneLimit,
            IKOverrideFly,
            IKOverrideJump,
            DetectEmotes,
            MassCenter
        };

        public static float CrouchLimit { get; private set; } = 0.75f;
        public static float ProneLimit { get; private set; } = 0.4f;
        public static bool IKOverrideFly { get; private set; } = true;
        public static bool IKOverrideJump { get; private set; } = true;
        public static bool DetectEmotes { get; private set; } = true;
        public static bool MassCenter { get; private set; } = true;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        public static readonly SettingEvent<float> OnCrouchLimitChanged = new SettingEvent<float>();
        public static readonly SettingEvent<float> OnProneLimitChanged = new SettingEvent<float>();
        public static readonly SettingEvent<bool> OnIKOverrideFlyChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnIKOverrideJumpChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnDetectEmotesChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnMassCenterChanged = new SettingEvent<bool>();

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("AMT", null, true);

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.CrouchLimit.ToString(), (int)(CrouchLimit * 100f)),
                ms_category.CreateEntry(ModSetting.ProneLimit.ToString(), (int)(ProneLimit * 100f)),
                ms_category.CreateEntry(ModSetting.IKOverrideFly.ToString(), IKOverrideFly),
                ms_category.CreateEntry(ModSetting.IKOverrideJump.ToString(), IKOverrideJump),
                ms_category.CreateEntry(ModSetting.DetectEmotes.ToString(), DetectEmotes),
                ms_category.CreateEntry(ModSetting.MassCenter.ToString(), MassCenter)
            };

            CrouchLimit = ((int)ms_entries[(int)ModSetting.CrouchLimit].BoxedValue) * 0.01f;
            ProneLimit = ((int)ms_entries[(int)ModSetting.ProneLimit].BoxedValue) * 0.01f;
            IKOverrideFly = (bool)ms_entries[(int)ModSetting.IKOverrideFly].BoxedValue;
            IKOverrideJump = (bool)ms_entries[(int)ModSetting.IKOverrideJump].BoxedValue;
            DetectEmotes = (bool)ms_entries[(int)ModSetting.DetectEmotes].BoxedValue;
            MassCenter = (bool)ms_entries[(int)ModSetting.MassCenter].BoxedValue;

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
                ViewManager.Instance.gameMenuView.View.BindCall("OnSliderUpdate_" + ms_category.Identifier, new Action<string, string>(OnSliderUpdate));
                ViewManager.Instance.gameMenuView.View.BindCall("OnToggleUpdate_" + ms_category.Identifier, new Action<string, string>(OnToggleUpdate));
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
            try
            {
                if(Enum.TryParse(p_name, out ModSetting l_setting))
                {
                    switch(l_setting)
                    {
                        case ModSetting.CrouchLimit:
                        {
                            CrouchLimit = int.Parse(p_value) * 0.01f;
                            OnCrouchLimitChanged.Invoke(CrouchLimit);
                        }
                        break;

                        case ModSetting.ProneLimit:
                        {
                            ProneLimit = int.Parse(p_value) * 0.01f;
                            OnProneLimitChanged.Invoke(ProneLimit);
                        }
                        break;
                    }

                    ms_entries[(int)l_setting].BoxedValue = int.Parse(p_value);
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
                if(Enum.TryParse(p_name, out ModSetting l_setting))
                {
                    switch(l_setting)
                    {
                        case ModSetting.IKOverrideFly:
                        {
                            IKOverrideFly = bool.Parse(p_value);
                            OnIKOverrideFlyChanged.Invoke(IKOverrideFly);
                        }
                        break;

                        case ModSetting.IKOverrideJump:
                        {
                            IKOverrideJump = bool.Parse(p_value);
                            OnIKOverrideJumpChanged.Invoke(IKOverrideJump);
                        }
                        break;

                        case ModSetting.DetectEmotes:
                        {
                            DetectEmotes = bool.Parse(p_value);
                            OnDetectEmotesChanged.Invoke(DetectEmotes);
                        }
                        break;

                        case ModSetting.MassCenter:
                        {
                            MassCenter = bool.Parse(p_value);
                            OnMassCenterChanged.Invoke(MassCenter);
                        }
                        break;
                    }

                    ms_entries[(int)l_setting].BoxedValue = bool.Parse(p_value);
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

using ABI_RC.Core.InteractionSystem;
using System;
using System.Collections.Generic;

namespace ml_vei
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
            Gestures = 0,
            GripTrigger,
            AxisPriority
        }

        public enum PriorityAxis
        {
            Grip = 0,
            Trigger
        }

        public static bool Gestures { get; private set; } = true;
        public static bool GripTrigger { get; private set; } = true;
        public static PriorityAxis AxisPriority { get; private set; } = PriorityAxis.Grip;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        public static readonly SettingEvent<bool> OnGesturesChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnGripTriggerChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<PriorityAxis> OnAxisPriorityChanged = new SettingEvent<PriorityAxis>();

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("VEI", null, true);

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.Gestures.ToString(), Gestures),
                ms_category.CreateEntry(ModSetting.GripTrigger.ToString(), GripTrigger),
                ms_category.CreateEntry(ModSetting.AxisPriority.ToString(), (int)AxisPriority),
            };

            Gestures = (bool)ms_entries[(int)ModSetting.Gestures].BoxedValue;
            GripTrigger = (bool)ms_entries[(int)ModSetting.GripTrigger].BoxedValue;
            AxisPriority = (PriorityAxis)(int)ms_entries[(int)ModSetting.AxisPriority].BoxedValue;

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
                ViewManager.Instance.gameMenuView.View.BindCall("OnDropdownUpdate_" + ms_category.Identifier, new Action<string, string>(OnDropdownUpdate));
            };
            ViewManager.Instance.gameMenuView.Listener.FinishLoad += (_) =>
            {
                ViewManager.Instance.gameMenuView.View.ExecuteScript(ResourcesHandler.GetEmbeddedResource("mods_extension.js"));
                ViewManager.Instance.gameMenuView.View.ExecuteScript(ResourcesHandler.GetEmbeddedResource("mod_menu.js"));
                MelonLoader.MelonCoroutines.Start(UpdateMenuSettings());
            };
        }

        static System.Collections.IEnumerator UpdateMenuSettings()
        {
            while(!ViewManager.Instance.IsReady || !ViewManager.Instance.IsMainMenuOpen)
                yield return null;

            foreach(var l_entry in ms_entries)
                ViewManager.Instance.gameMenuView.View.TriggerEvent("updateModSetting", ms_category.Identifier, l_entry.DisplayName, l_entry.GetValueAsString());
        }

        static void OnToggleUpdate(string p_name, string p_value)
        {
            try
            {
                if(Enum.TryParse(p_name, out ModSetting l_setting) && bool.TryParse(p_value,out bool l_value))
                {
                    switch(l_setting)
                    {
                        case ModSetting.Gestures:
                        {
                            Gestures = l_value;
                            OnGesturesChanged.Invoke(Gestures);
                        }
                        break;

                        case ModSetting.GripTrigger:
                        {
                            GripTrigger = l_value;
                            OnGripTriggerChanged.Invoke(GripTrigger);
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

        static void OnDropdownUpdate(string p_name, string p_value)
        {
            try
            {
                if(Enum.TryParse(p_name, out ModSetting l_setting) && int.TryParse(p_value, out int l_value))
                {
                    switch(l_setting)
                    {
                        case ModSetting.AxisPriority:
                        {
                            AxisPriority = (PriorityAxis)l_value;
                            OnAxisPriorityChanged.Invoke(AxisPriority);
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

using ABI_RC.Core.InteractionSystem;
using System;
using System.Collections.Generic;

namespace ml_vei
{
    static class Settings
    {
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

        static public event Action<bool> GesturesChange;
        static public event Action<bool> GripTriggerChange;
        static public event Action<PriorityAxis> AxisPriorityChange;

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
                foreach(var l_entry in ms_entries)
                    ViewManager.Instance.gameMenuView.View.TriggerEvent("updateModSetting", ms_category.Identifier, l_entry.DisplayName, l_entry.GetValueAsString());
            };
        }

        static void OnToggleUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.Gestures:
                    {
                        Gestures = bool.Parse(p_value);
                        GesturesChange?.Invoke(Gestures);
                    }
                    break;

                    case ModSetting.GripTrigger:
                    {
                        GripTrigger = bool.Parse(p_value);
                        GripTriggerChange?.Invoke(GripTrigger);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = bool.Parse(p_value);
            }
        }

        static void OnDropdownUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.AxisPriority:
                    {
                        AxisPriority = (PriorityAxis)int.Parse(p_value);
                        AxisPriorityChange?.Invoke(AxisPriority);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = int.Parse(p_value);
            }
        }
    }
}

using ABI_RC.Core.InteractionSystem;
using System;
using System.Collections.Generic;

namespace ml_asl
{
    static class Settings
    {
        public enum ModSetting
        {
            Enabled = 0
        }

        public static bool Enabled { get; private set; } = true;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        public static event Action<bool> EnabledChange;

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("ASL", null, true);

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.Enabled.ToString(), Enabled)
            };

            Enabled = (bool)ms_entries[(int)ModSetting.Enabled].BoxedValue;

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
                    case ModSetting.Enabled:
                    {
                        Enabled = bool.Parse(p_value);
                        EnabledChange?.Invoke(Enabled);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = bool.Parse(p_value);
            }
        }
    }
}

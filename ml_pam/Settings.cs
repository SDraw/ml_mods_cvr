using ABI_RC.Core.InteractionSystem;
using System;
using System.Collections.Generic;

namespace ml_pam
{
    static class Settings
    {
        public enum ModSetting
        {
            Enabled = 0,
            GrabOffset,
            LeadHand,
            HandsExtension
        }
        public enum LeadHand
        {
            Left = 0,
            Right,
            Both
        }

        public static bool Enabled { get; private set; } = true;
        public static float GrabOffset { get; private set; } = 0.25f;
        public static LeadHand LeadingHand { get; private set; } = LeadHand.Right;
        public static bool HandsExtension { get; private set; } = true;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        public static event Action<bool> EnabledChange;
        public static event Action<float> GrabOffsetChange;
        public static event Action<LeadHand> LeadingHandChange;
        public static event Action<bool> HandsExtensionChange;

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("PAM", null, true);

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.Enabled.ToString(), Enabled),
                ms_category.CreateEntry(ModSetting.GrabOffset.ToString(), (int)(GrabOffset * 100f)),
                ms_category.CreateEntry(ModSetting.LeadHand.ToString(), (int)LeadHand.Right),
                ms_category.CreateEntry(ModSetting.HandsExtension.ToString(), HandsExtension),
            };

            Enabled = (bool)ms_entries[(int)ModSetting.Enabled].BoxedValue;
            GrabOffset = (int)ms_entries[(int)ModSetting.GrabOffset].BoxedValue * 0.01f;
            LeadingHand = (LeadHand)(int)ms_entries[(int)ModSetting.LeadHand].BoxedValue;
            HandsExtension = (bool)ms_entries[(int)ModSetting.HandsExtension].BoxedValue;

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
                ViewManager.Instance.gameMenuView.View.BindCall("OnSliderUpdate_" + ms_category.Identifier, new Action<string, string>(OnSliderUpdate));
                ViewManager.Instance.gameMenuView.View.BindCall("OnDropdownUpdate_" + ms_category.Identifier, new Action<string, string>(OnDropdownUpdate));
            };
            ViewManager.Instance.gameMenuView.Listener.FinishLoad += (_) =>
            {
                ViewManager.Instance.gameMenuView.View.ExecuteScript(ResourcesHandler.GetEmbeddedResources("mods_extension.js"));
                ViewManager.Instance.gameMenuView.View.ExecuteScript(ResourcesHandler.GetEmbeddedResources("mod_menu.js"));
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

                    case ModSetting.HandsExtension:
                    {
                        HandsExtension = bool.Parse(p_value);
                        HandsExtensionChange?.Invoke(HandsExtension);
                    } break;
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
                        GrabOffset = int.Parse(p_value) * 0.01f;
                        GrabOffsetChange?.Invoke(GrabOffset);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = int.Parse(p_value);
            }
        }

        static void OnDropdownUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.LeadHand:
                    {
                        LeadingHand = (LeadHand)int.Parse(p_value);
                        LeadingHandChange?.Invoke(LeadingHand);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = int.Parse(p_value);
            }
        }
    }
}

using ABI_RC.Core.InteractionSystem;
using System;
using System.Collections.Generic;

namespace ml_bft
{
    static class Settings
    {
        enum ModSetting
        {
            SkeletalInput = 0,
            ShowHands
        }

        public static bool SkeletalInput { get; private set; } = false;
        public static bool ShowHands { get; private set; } = false;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        static public event Action<bool> SkeletalInputChange;
        static public event Action<bool> ShowHandsChange;

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("BFT", null, true);

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.SkeletalInput.ToString(), SkeletalInput),
                ms_category.CreateEntry(ModSetting.ShowHands.ToString(), ShowHands)
            };

            SkeletalInput = (bool)ms_entries[(int)ModSetting.SkeletalInput].BoxedValue;
            ShowHands = (bool)ms_entries[(int)ModSetting.ShowHands].BoxedValue;

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
                    case ModSetting.SkeletalInput:
                    {
                        SkeletalInput = bool.Parse(p_value);
                        SkeletalInputChange?.Invoke(SkeletalInput);
                    }
                    break;
                    
                    case ModSetting.ShowHands:
                    {
                        ShowHands = bool.Parse(p_value);
                        ShowHandsChange?.Invoke(ShowHands);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = bool.Parse(p_value);
            }
        }
    }
}

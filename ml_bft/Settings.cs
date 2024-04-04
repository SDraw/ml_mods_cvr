using ABI_RC.Core.InteractionSystem;
using System;
using System.Collections.Generic;

namespace ml_bft
{
    static class Settings
    {
        public enum MotionRangeType
        {
            WithController = 0,
            WithoutController
        }
        enum ModSetting
        {
            SkeletalInput = 0,
            MotionRange,
            ShowHands,
            MechanimFilter
        }

        public static bool SkeletalInput { get; private set; } = false;
        public static MotionRangeType MotionRange { get; private set; } = MotionRangeType.WithController;
        public static bool ShowHands { get; private set; } = false;
        public static bool MechanimFilter { get; private set; } = true;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        public static event Action<bool> SkeletalInputChange;
        public static event Action<MotionRangeType> MotionRangeChange;
        public static event Action<bool> ShowHandsChange;
        public static event Action<bool> MechanimFilterChange;

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("BFT", null, true);

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.SkeletalInput.ToString(), SkeletalInput),
                ms_category.CreateEntry(ModSetting.MotionRange.ToString(), (int)MotionRange),
                ms_category.CreateEntry(ModSetting.ShowHands.ToString(), ShowHands),
                ms_category.CreateEntry(ModSetting.MechanimFilter.ToString(), MechanimFilter)
            };

            SkeletalInput = (bool)ms_entries[(int)ModSetting.SkeletalInput].BoxedValue;
            MotionRange = (MotionRangeType)(int)ms_entries[(int)ModSetting.MotionRange].BoxedValue;
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
                    
                    case ModSetting.MechanimFilter:
                    {
                        MechanimFilter = bool.Parse(p_value);
                        MechanimFilterChange?.Invoke(MechanimFilter);
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
                    case ModSetting.MotionRange:
                    {
                        MotionRange = (MotionRangeType)int.Parse(p_value);
                        MotionRangeChange?.Invoke(MotionRange);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = int.Parse(p_value);
            }
        }
    }
}

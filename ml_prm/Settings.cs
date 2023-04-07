using System;
using System.Collections.Generic;
using System.Linq;

namespace ml_prm
{
    static class Settings
    {
        public enum ModSetting
        {
            Hotkey = 0,
            Multiplier,
            RestorePosition
        }

        public static bool Hotkey { get; private set; } = true;
        public static float Multiplier { get; private set; } = 2f;
        public static bool RestorePosition { get; private set; } = false;

        static public event Action SwitchChange;
        static public event Action<bool> HotkeyChange;
        static public event Action<bool> RestorePositionChange;
        static public event Action<float> MultiplierChange;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("PRM");
            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.Hotkey.ToString(), Hotkey),
                ms_category.CreateEntry(ModSetting.Multiplier.ToString(), Multiplier),
                ms_category.CreateEntry(ModSetting.RestorePosition.ToString(), RestorePosition),
            };

            Hotkey = (bool)ms_entries[(int)ModSetting.Hotkey].BoxedValue;
            Multiplier = (float)ms_entries[(int)ModSetting.Multiplier].BoxedValue;
            RestorePosition = (bool)ms_entries[(int)ModSetting.RestorePosition].BoxedValue;

            if(MelonLoader.MelonMod.RegisteredMelons.First(m => m.Info.Name == "BTKUILib") != null)
            {
                CreateBtkUi();
            }
        }

        static void CreateBtkUi()
        {
            var l_categoryMain = BTKUILib.QuickMenuAPI.MiscTabPage.AddCategory("PlayerRagdollMod");
            var l_page = l_categoryMain.AddPage("Player Ragdoll Settings", "", "PlayerRagdollMod settings", "PlayerRagdollMod");
            l_page.MenuTitle = "Ragdoll settings";
            var l_categoryMod = l_page.AddCategory("Settings");

            l_categoryMod.AddButton("Switch ragdoll", "", "Switch between normal and ragdoll state").OnPress += () =>
            {
                SwitchChange?.Invoke();
            };
            l_categoryMod.AddToggle("Use hotkey", "Switch ragdoll mode with 'R' key", Hotkey).OnValueUpdated += (state) =>
            {
                Hotkey = state;
                ms_entries[(int)ModSetting.Hotkey].BoxedValue = state;
                HotkeyChange?.Invoke(Hotkey);
            };
            l_categoryMod.AddToggle("Restore position", "Bring avatar back where ragdoll state was activated", RestorePosition).OnValueUpdated += (state) =>
            {
                RestorePosition = state;
                ms_entries[(int)ModSetting.RestorePosition].BoxedValue = state;
                RestorePositionChange?.Invoke(Hotkey);
            };
            l_page.AddSlider("Velocity multiplier", "Velocity multiplier upon entering ragdoll state", Multiplier, 1f, 50f).OnValueUpdated += (value) =>
            {
                Multiplier = value;
                ms_entries[(int)ModSetting.Multiplier].BoxedValue = value;
                MultiplierChange?.Invoke(value);
            };
        }
    }
}

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
            VelocityMultiplier,
            RestorePosition,
            MovementDrag,
            AngularDrag,
            Gravity
        }

        enum UiElementIndex
        {
            Hotkey = 0,
            RestorePosition,
            Gravity,
            VelocityMultiplier,
            MovementDrag,
            AngularDrag
        }

        public static bool Hotkey { get; private set; } = true;
        public static float VelocityMultiplier { get; private set; } = 2f;
        public static bool RestorePosition { get; private set; } = false;
        public static float MovementDrag { get; private set; } = 1f;
        public static float AngularDrag { get; private set; } = 0.5f;
        public static bool Gravity { get; private set; } = true;

        static public event Action SwitchChange;
        static public event Action<bool> HotkeyChange;
        static public event Action<bool> RestorePositionChange;
        static public event Action<float> VelocityMultiplierChange;
        static public event Action<float> MovementDragChange;
        static public event Action<float> AngularDragChange;
        static public event Action<bool> GravityChange;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        static List<object> ms_uiElements = new List<object>();

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("PRM");
            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.Hotkey.ToString(), Hotkey),
                ms_category.CreateEntry(ModSetting.VelocityMultiplier.ToString(), VelocityMultiplier),
                ms_category.CreateEntry(ModSetting.RestorePosition.ToString(), RestorePosition),
                ms_category.CreateEntry(ModSetting.MovementDrag.ToString(), MovementDrag),
                ms_category.CreateEntry(ModSetting.AngularDrag.ToString(), AngularDrag),
                ms_category.CreateEntry(ModSetting.Gravity.ToString(), Gravity),
            };

            Hotkey = (bool)ms_entries[(int)ModSetting.Hotkey].BoxedValue;
            VelocityMultiplier = UnityEngine.Mathf.Clamp((float)ms_entries[(int)ModSetting.VelocityMultiplier].BoxedValue, 0f, 50f);
            RestorePosition = (bool)ms_entries[(int)ModSetting.RestorePosition].BoxedValue;
            MovementDrag = UnityEngine.Mathf.Clamp((float)ms_entries[(int)ModSetting.MovementDrag].BoxedValue, 0f, 100f);
            AngularDrag = UnityEngine.Mathf.Clamp((float)ms_entries[(int)ModSetting.MovementDrag].BoxedValue, 0.5f, 50f);
            Gravity = (bool)ms_entries[(int)ModSetting.Gravity].BoxedValue;

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

            ms_uiElements.Add(l_categoryMod.AddToggle("Use hotkey", "Switch ragdoll mode with 'R' key", Hotkey));
            (ms_uiElements[(int)UiElementIndex.Hotkey] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) =>
            {
                Hotkey = state;
                ms_entries[(int)ModSetting.Hotkey].BoxedValue = state;
                HotkeyChange?.Invoke(state);
            };

            ms_uiElements.Add(l_categoryMod.AddToggle("Restore position", "Bring avatar back where ragdoll state was activated", RestorePosition));
            (ms_uiElements[(int)UiElementIndex.RestorePosition] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) =>
            {
                RestorePosition = state;
                ms_entries[(int)ModSetting.RestorePosition].BoxedValue = state;
                RestorePositionChange?.Invoke(state);
            };

            ms_uiElements.Add(l_categoryMod.AddToggle("Use gravity", "Apply gravity to ragdoll", Gravity));
            (ms_uiElements[(int)UiElementIndex.Gravity] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) =>
            {
                Gravity = state;
                ms_entries[(int)ModSetting.Gravity].BoxedValue = state;
                GravityChange?.Invoke(state);
            };

            ms_uiElements.Add(l_page.AddSlider("Velocity multiplier", "Velocity multiplier upon entering ragdoll state", VelocityMultiplier, 1f, 50f));
            (ms_uiElements[(int)UiElementIndex.VelocityMultiplier] as BTKUILib.UIObjects.Components.SliderFloat).OnValueUpdated += (value) =>
            {
                VelocityMultiplier = value;
                ms_entries[(int)ModSetting.VelocityMultiplier].BoxedValue = value;
                VelocityMultiplierChange?.Invoke(value);
            };

            ms_uiElements.Add(l_page.AddSlider("Movement drag", "Movement resistance", MovementDrag, 0f, 100f));
            (ms_uiElements[(int)UiElementIndex.MovementDrag] as BTKUILib.UIObjects.Components.SliderFloat).OnValueUpdated += (value) =>
            {
                MovementDrag = value;
                ms_entries[(int)ModSetting.MovementDrag].BoxedValue = value;
                MovementDragChange?.Invoke(value);
            };

            ms_uiElements.Add(l_page.AddSlider("Angular movement drag", "Rotation movement resistance", AngularDrag, 0.5f, 50f));
            (ms_uiElements[(int)UiElementIndex.AngularDrag] as BTKUILib.UIObjects.Components.SliderFloat).OnValueUpdated += (value) =>
            {
                AngularDrag = value;
                ms_entries[(int)ModSetting.AngularDrag].BoxedValue = value;
                AngularDragChange?.Invoke(AngularDrag);
            };

            l_categoryMod.AddButton("Reset settings", "", "Reset mod settings to default").OnPress += () =>
            {
                Hotkey = true;
                (ms_uiElements[(int)UiElementIndex.Hotkey] as BTKUILib.UIObjects.Components.ToggleButton).ToggleValue = true;
                ms_entries[(int)ModSetting.Hotkey].BoxedValue = true;
                HotkeyChange?.Invoke(true);

                RestorePosition = false;
                ms_entries[(int)ModSetting.RestorePosition].BoxedValue = false;
                (ms_uiElements[(int)UiElementIndex.RestorePosition] as BTKUILib.UIObjects.Components.ToggleButton).ToggleValue = false;
                RestorePositionChange?.Invoke(false);

                Gravity = true;
                ms_entries[(int)ModSetting.Gravity].BoxedValue = true;
                (ms_uiElements[(int)UiElementIndex.Gravity] as BTKUILib.UIObjects.Components.ToggleButton).ToggleValue = true;
                GravityChange?.Invoke(true);

                VelocityMultiplier = 2f;
                ms_entries[(int)ModSetting.VelocityMultiplier].BoxedValue = 2f;
                (ms_uiElements[(int)UiElementIndex.VelocityMultiplier] as BTKUILib.UIObjects.Components.SliderFloat).SetSliderValue(2f);
                VelocityMultiplierChange?.Invoke(2f);

                MovementDrag = 1f;
                ms_entries[(int)ModSetting.MovementDrag].BoxedValue = 1f;
                (ms_uiElements[(int)UiElementIndex.MovementDrag] as BTKUILib.UIObjects.Components.SliderFloat).SetSliderValue(1f);
                MovementDragChange?.Invoke(1f);

                AngularDrag = 0.5f;
                ms_entries[(int)ModSetting.MovementDrag].BoxedValue = 0.5f;
                (ms_uiElements[(int)UiElementIndex.AngularDrag] as BTKUILib.UIObjects.Components.SliderFloat).SetSliderValue(0.5f);
                AngularDragChange?.Invoke(0.5f);
            };
        }
    }
}

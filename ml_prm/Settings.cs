using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ml_prm
{
    static class Settings
    {
        enum ModSetting
        {
            Hotkey = 0,
            VelocityMultiplier,
            RestorePosition,
            MovementDrag,
            AngularDrag,
            Gravity,
            PointersReaction,
            IgnoreLocal,
            CombatReaction,
            AutoRecover,
            RecoverDelay,
            Slipperiness,
            Bounciness
        }

        enum UiElementIndex
        {
            Hotkey = 0,
            RestorePosition,
            Gravity,
            PointersReaction,
            IgnoreLocal,
            CombatReaction,
            AutoRecover,
            Slipperiness,
            Bounciness,
            VelocityMultiplier,
            MovementDrag,
            AngularDrag,
            RecoverDelay
        }

        public static bool Hotkey { get; private set; } = true;
        public static float VelocityMultiplier { get; private set; } = 2f;
        public static bool RestorePosition { get; private set; } = false;
        public static float MovementDrag { get; private set; } = 2f;
        public static float AngularDrag { get; private set; } = 2f;
        public static bool Gravity { get; private set; } = true;
        public static bool PointersReaction { get; private set; } = true;
        public static bool IgnoreLocal { get; private set; } = true;
        public static bool CombatReaction { get; private set; } = true;
        public static bool AutoRecover { get; private set; } = false;
        public static float RecoverDelay { get; private set; } = 3f;
        public static bool Slipperiness { get; private set; } = false;
        public static bool Bounciness { get; private set; } = false;

        static public event Action SwitchChange;
        static public event Action<bool> HotkeyChange;
        static public event Action<bool> RestorePositionChange;
        static public event Action<float> VelocityMultiplierChange;
        static public event Action<float> MovementDragChange;
        static public event Action<float> AngularDragChange;
        static public event Action<bool> GravityChange;
        static public event Action<bool> PointersReactionChange;
        static public event Action<bool> IgnoreLocalChange;
        static public event Action<bool> CombatReactionChange;
        static public event Action<bool> AutoRecoverChange;
        static public event Action<float> RecoverDelayChange;
        static public event Action<bool> SlipperinessChange;
        static public event Action<bool> BouncinessChange;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        static List<object> ms_uiElements = new List<object>();

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("PRM", null, true);
            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.Hotkey.ToString(), Hotkey),
                ms_category.CreateEntry(ModSetting.VelocityMultiplier.ToString(), VelocityMultiplier),
                ms_category.CreateEntry(ModSetting.RestorePosition.ToString(), RestorePosition),
                ms_category.CreateEntry(ModSetting.MovementDrag.ToString(), MovementDrag),
                ms_category.CreateEntry(ModSetting.AngularDrag.ToString(), AngularDrag),
                ms_category.CreateEntry(ModSetting.Gravity.ToString(), Gravity),
                ms_category.CreateEntry(ModSetting.PointersReaction.ToString(), PointersReaction),
                ms_category.CreateEntry(ModSetting.IgnoreLocal.ToString(), IgnoreLocal),
                ms_category.CreateEntry(ModSetting.CombatReaction.ToString(), CombatReaction),
                ms_category.CreateEntry(ModSetting.AutoRecover.ToString(), AutoRecover),
                ms_category.CreateEntry(ModSetting.RecoverDelay.ToString(), RecoverDelay),
                ms_category.CreateEntry(ModSetting.Slipperiness.ToString(), Slipperiness),
                ms_category.CreateEntry(ModSetting.Bounciness.ToString(), Bounciness)
            };

            Hotkey = (bool)ms_entries[(int)ModSetting.Hotkey].BoxedValue;
            VelocityMultiplier = Mathf.Clamp((float)ms_entries[(int)ModSetting.VelocityMultiplier].BoxedValue, 1f, 50f);
            RestorePosition = (bool)ms_entries[(int)ModSetting.RestorePosition].BoxedValue;
            MovementDrag = Mathf.Clamp((float)ms_entries[(int)ModSetting.MovementDrag].BoxedValue, 0f, 50f);
            AngularDrag = Mathf.Clamp((float)ms_entries[(int)ModSetting.MovementDrag].BoxedValue, 0f, 50f);
            Gravity = (bool)ms_entries[(int)ModSetting.Gravity].BoxedValue;
            PointersReaction = (bool)ms_entries[(int)ModSetting.PointersReaction].BoxedValue;
            IgnoreLocal = (bool)ms_entries[(int)ModSetting.IgnoreLocal].BoxedValue;
            CombatReaction = (bool)ms_entries[(int)ModSetting.CombatReaction].BoxedValue;
            AutoRecover = (bool)ms_entries[(int)ModSetting.AutoRecover].BoxedValue;
            RecoverDelay = Mathf.Clamp((float)ms_entries[(int)ModSetting.RecoverDelay].BoxedValue, 1f, 10f);
            Slipperiness = (bool)ms_entries[(int)ModSetting.Slipperiness].BoxedValue;
            Bounciness = (bool)ms_entries[(int)ModSetting.Bounciness].BoxedValue;

            if(MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m.Info.Name == "BTKUILib") != null)
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

            ms_uiElements.Add(l_categoryMod.AddToggle("Pointers reaction", "React to trigger colliders with CVRPointer component of 'ragdoll' type", PointersReaction));
            (ms_uiElements[(int)UiElementIndex.PointersReaction] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) =>
            {
                PointersReaction = state;
                ms_entries[(int)ModSetting.PointersReaction].BoxedValue = state;
                PointersReactionChange?.Invoke(state);
            };

            ms_uiElements.Add(l_categoryMod.AddToggle("Ignore local pointers", "Ignore local avatar's CVRPointer components of 'ragdoll' type", IgnoreLocal));
            (ms_uiElements[(int)UiElementIndex.IgnoreLocal] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) =>
            {
                IgnoreLocal = state;
                ms_entries[(int)ModSetting.IgnoreLocal].BoxedValue = state;
                IgnoreLocalChange?.Invoke(state);
            };

            ms_uiElements.Add(l_categoryMod.AddToggle("Combat reaction", "Ragdoll upon combat system death", CombatReaction));
            (ms_uiElements[(int)UiElementIndex.CombatReaction] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) =>
            {
                CombatReaction = state;
                ms_entries[(int)ModSetting.CombatReaction].BoxedValue = state;
                CombatReactionChange?.Invoke(state);
            };

            ms_uiElements.Add(l_categoryMod.AddToggle("Auto recover", "Automatically unragdoll after set recover delay", AutoRecover));
            (ms_uiElements[(int)UiElementIndex.AutoRecover] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) =>
            {
                AutoRecover = state;
                ms_entries[(int)ModSetting.AutoRecover].BoxedValue = state;
                AutoRecoverChange?.Invoke(state);
            };

            ms_uiElements.Add(l_categoryMod.AddToggle("Slipperiness", "Enables/disables friction of ragdoll", Slipperiness));
            (ms_uiElements[(int)UiElementIndex.Slipperiness] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) =>
            {
                Slipperiness = state;
                ms_entries[(int)ModSetting.Slipperiness].BoxedValue = state;
                SlipperinessChange?.Invoke(state);
            };

            ms_uiElements.Add(l_categoryMod.AddToggle("Bounciness", "Enables/disables bounciness of ragdoll", Bounciness));
            (ms_uiElements[(int)UiElementIndex.Bounciness] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) =>
            {
                Bounciness = state;
                ms_entries[(int)ModSetting.Bounciness].BoxedValue = state;
                BouncinessChange?.Invoke(state);
            };

            ms_uiElements.Add(l_page.AddSlider("Velocity multiplier", "Velocity multiplier upon entering ragdoll state", VelocityMultiplier, 1f, 50f));
            (ms_uiElements[(int)UiElementIndex.VelocityMultiplier] as BTKUILib.UIObjects.Components.SliderFloat).OnValueUpdated += (value) =>
            {
                VelocityMultiplier = value;
                ms_entries[(int)ModSetting.VelocityMultiplier].BoxedValue = value;
                VelocityMultiplierChange?.Invoke(value);
            };

            ms_uiElements.Add(l_page.AddSlider("Movement drag", "Movement resistance", MovementDrag, 0f, 50f));
            (ms_uiElements[(int)UiElementIndex.MovementDrag] as BTKUILib.UIObjects.Components.SliderFloat).OnValueUpdated += (value) =>
            {
                MovementDrag = value;
                ms_entries[(int)ModSetting.MovementDrag].BoxedValue = value;
                MovementDragChange?.Invoke(value);
            };

            ms_uiElements.Add(l_page.AddSlider("Angular movement drag", "Rotation movement resistance", AngularDrag, 0f, 50f));
            (ms_uiElements[(int)UiElementIndex.AngularDrag] as BTKUILib.UIObjects.Components.SliderFloat).OnValueUpdated += (value) =>
            {
                AngularDrag = value;
                ms_entries[(int)ModSetting.AngularDrag].BoxedValue = value;
                AngularDragChange?.Invoke(value);
            };

            ms_uiElements.Add(l_page.AddSlider("Recover delay (seconds)", "Recover delay for automatic recover", RecoverDelay, 1f, 10f));
            (ms_uiElements[(int)UiElementIndex.RecoverDelay] as BTKUILib.UIObjects.Components.SliderFloat).OnValueUpdated += (value) =>
            {
                RecoverDelay = value;
                ms_entries[(int)ModSetting.RecoverDelay].BoxedValue = value;
                RecoverDelayChange?.Invoke(value);
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

                PointersReaction = true;
                ms_entries[(int)ModSetting.PointersReaction].BoxedValue = true;
                (ms_uiElements[(int)UiElementIndex.PointersReaction] as BTKUILib.UIObjects.Components.ToggleButton).ToggleValue = true;
                PointersReactionChange?.Invoke(true);

                IgnoreLocal = true;
                ms_entries[(int)ModSetting.IgnoreLocal].BoxedValue = true;
                (ms_uiElements[(int)UiElementIndex.IgnoreLocal] as BTKUILib.UIObjects.Components.ToggleButton).ToggleValue = true;
                IgnoreLocalChange?.Invoke(true);

                CombatReaction = true;
                ms_entries[(int)ModSetting.CombatReaction].BoxedValue = true;
                (ms_uiElements[(int)UiElementIndex.CombatReaction] as BTKUILib.UIObjects.Components.ToggleButton).ToggleValue = true;
                CombatReactionChange?.Invoke(true);

                AutoRecover = false;
                ms_entries[(int)ModSetting.AutoRecover].BoxedValue = false;
                (ms_uiElements[(int)UiElementIndex.AutoRecover] as BTKUILib.UIObjects.Components.ToggleButton).ToggleValue = false;
                AutoRecoverChange?.Invoke(false);

                Slipperiness = false;
                ms_entries[(int)ModSetting.Slipperiness].BoxedValue = false;
                (ms_uiElements[(int)UiElementIndex.Slipperiness] as BTKUILib.UIObjects.Components.ToggleButton).ToggleValue = false;
                SlipperinessChange?.Invoke(false);

                Bounciness = false;
                ms_entries[(int)ModSetting.Bounciness].BoxedValue = false;
                (ms_uiElements[(int)UiElementIndex.Bounciness] as BTKUILib.UIObjects.Components.ToggleButton).ToggleValue = false;
                BouncinessChange?.Invoke(false);

                VelocityMultiplier = 2f;
                ms_entries[(int)ModSetting.VelocityMultiplier].BoxedValue = 2f;
                (ms_uiElements[(int)UiElementIndex.VelocityMultiplier] as BTKUILib.UIObjects.Components.SliderFloat).SetSliderValue(2f);
                VelocityMultiplierChange?.Invoke(2f);

                MovementDrag = 2f;
                ms_entries[(int)ModSetting.MovementDrag].BoxedValue = 2f;
                (ms_uiElements[(int)UiElementIndex.MovementDrag] as BTKUILib.UIObjects.Components.SliderFloat).SetSliderValue(2f);
                MovementDragChange?.Invoke(2f);

                AngularDrag = 2f;
                ms_entries[(int)ModSetting.AngularDrag].BoxedValue = 2f;
                (ms_uiElements[(int)UiElementIndex.AngularDrag] as BTKUILib.UIObjects.Components.SliderFloat).SetSliderValue(2f);
                AngularDragChange?.Invoke(2f);

                RecoverDelay = 3f;
                ms_entries[(int)ModSetting.RecoverDelay].BoxedValue = 3f;
                (ms_uiElements[(int)UiElementIndex.RecoverDelay] as BTKUILib.UIObjects.Components.SliderFloat).SetSliderValue(3f);
                RecoverDelayChange?.Invoke(3f);
            };
        }
    }
}

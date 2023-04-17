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
            Bounciness,
            ViewVelocity,
            VrFollow,
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
            ViewVelocity,
            VrFollow,
            VelocityMultiplier,
            MovementDrag,
            AngularDrag,
            RecoverDelay,

            Count
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
        public static bool ViewVelocity { get; private set; } = false;
        public static bool VrFollow { get; private set; } = true;

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
        static public event Action<bool> ViewVelocityChange;
        static public event Action<bool> VrFollowChange;

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
                ms_category.CreateEntry(ModSetting.Bounciness.ToString(), Bounciness),
                ms_category.CreateEntry(ModSetting.ViewVelocity.ToString(), ViewVelocity),
                ms_category.CreateEntry(ModSetting.VrFollow.ToString(), VrFollow)
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
            ViewVelocity = (bool)ms_entries[(int)ModSetting.ViewVelocity].BoxedValue;
            VrFollow = (bool)ms_entries[(int)ModSetting.VrFollow].BoxedValue;

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

            l_categoryMod.AddButton("Switch ragdoll", "", "Switch between normal and ragdoll state").OnPress += () => SwitchChange?.Invoke();

            ms_uiElements.Add(l_categoryMod.AddToggle("Use hotkey", "Switch ragdoll mode with 'R' key", Hotkey));
            (ms_uiElements[(int)UiElementIndex.Hotkey] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) => OnToggleUpdate(ModSetting.Hotkey, state);

            ms_uiElements.Add(l_categoryMod.AddToggle("Restore position", "Bring avatar back where ragdoll state was activated", RestorePosition));
            (ms_uiElements[(int)UiElementIndex.RestorePosition] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) => OnToggleUpdate(ModSetting.RestorePosition, state);

            ms_uiElements.Add(l_categoryMod.AddToggle("Use gravity", "Apply gravity to ragdoll", Gravity));
            (ms_uiElements[(int)UiElementIndex.Gravity] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) => OnToggleUpdate(ModSetting.Gravity, state);

            ms_uiElements.Add(l_categoryMod.AddToggle("Pointers reaction", "React to trigger colliders with CVRPointer component of 'ragdoll' type", PointersReaction));
            (ms_uiElements[(int)UiElementIndex.PointersReaction] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) => OnToggleUpdate(ModSetting.PointersReaction, state);

            ms_uiElements.Add(l_categoryMod.AddToggle("Ignore local pointers", "Ignore local avatar's CVRPointer components of 'ragdoll' type", IgnoreLocal));
            (ms_uiElements[(int)UiElementIndex.IgnoreLocal] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) => OnToggleUpdate(ModSetting.IgnoreLocal, state);

            ms_uiElements.Add(l_categoryMod.AddToggle("Combat reaction", "Ragdoll upon combat system death", CombatReaction));
            (ms_uiElements[(int)UiElementIndex.CombatReaction] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) => OnToggleUpdate(ModSetting.CombatReaction, state);

            ms_uiElements.Add(l_categoryMod.AddToggle("Auto recover", "Automatically unragdoll after set recover delay", AutoRecover));
            (ms_uiElements[(int)UiElementIndex.AutoRecover] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) => OnToggleUpdate(ModSetting.AutoRecover, state);

            ms_uiElements.Add(l_categoryMod.AddToggle("Slipperiness", "Enables/disables friction of ragdoll", Slipperiness));
            (ms_uiElements[(int)UiElementIndex.Slipperiness] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) => OnToggleUpdate(ModSetting.Slipperiness, state);

            ms_uiElements.Add(l_categoryMod.AddToggle("Bounciness", "Enables/disables bounciness of ragdoll", Bounciness));
            (ms_uiElements[(int)UiElementIndex.Bounciness] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) => OnToggleUpdate(ModSetting.Bounciness, state);

            ms_uiElements.Add(l_categoryMod.AddToggle("View direction velocity", "Apply velocity to camera view direction", ViewVelocity));
            (ms_uiElements[(int)UiElementIndex.ViewVelocity] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) => OnToggleUpdate(ModSetting.ViewVelocity, state);

            ms_uiElements.Add(l_categoryMod.AddToggle("VR camera follow", "Forces VR camera to follow ragdoll", VrFollow));
            (ms_uiElements[(int)UiElementIndex.VrFollow] as BTKUILib.UIObjects.Components.ToggleButton).OnValueUpdated += (state) => OnToggleUpdate( ModSetting.VrFollow, state);

            ms_uiElements.Add(l_page.AddSlider("Velocity multiplier", "Velocity multiplier upon entering ragdoll state", VelocityMultiplier, 1f, 50f));
            (ms_uiElements[(int)UiElementIndex.VelocityMultiplier] as BTKUILib.UIObjects.Components.SliderFloat).OnValueUpdated += (value) => OnSliderUpdate(ModSetting.VelocityMultiplier, value);

            ms_uiElements.Add(l_page.AddSlider("Movement drag", "Movement resistance", MovementDrag, 0f, 50f));
            (ms_uiElements[(int)UiElementIndex.MovementDrag] as BTKUILib.UIObjects.Components.SliderFloat).OnValueUpdated += (value) => OnSliderUpdate(ModSetting.MovementDrag, value);

            ms_uiElements.Add(l_page.AddSlider("Angular movement drag", "Rotation movement resistance", AngularDrag, 0f, 50f));
            (ms_uiElements[(int)UiElementIndex.AngularDrag] as BTKUILib.UIObjects.Components.SliderFloat).OnValueUpdated += (value) => OnSliderUpdate(ModSetting.AngularDrag, value);

            ms_uiElements.Add(l_page.AddSlider("Recover delay (seconds)", "Recover delay for automatic recover", RecoverDelay, 1f, 10f));
            (ms_uiElements[(int)UiElementIndex.RecoverDelay] as BTKUILib.UIObjects.Components.SliderFloat).OnValueUpdated += (value) => OnSliderUpdate(ModSetting.RecoverDelay, value);

            l_categoryMod.AddButton("Reset settings", "", "Reset mod settings to default").OnPress += Reset;
        }

        static void OnToggleUpdate(ModSetting p_setting, bool p_state, UiElementIndex p_uiIndex = UiElementIndex.Count)
        {
            switch(p_setting)
            {
                case ModSetting.Hotkey:
                {
                    Hotkey = p_state;
                    HotkeyChange?.Invoke(p_state);
                }
                break;

                case ModSetting.RestorePosition:
                {
                    RestorePosition = p_state;
                    RestorePositionChange?.Invoke(p_state);
                }
                break;

                case ModSetting.Gravity:
                {
                    Gravity = p_state;
                    GravityChange?.Invoke(p_state);
                }
                break;

                case ModSetting.PointersReaction:
                {
                    PointersReaction = p_state;
                    PointersReactionChange?.Invoke(p_state);
                } break;

                case ModSetting.IgnoreLocal:
                {
                    IgnoreLocal = p_state;
                    IgnoreLocalChange?.Invoke(p_state);
                } break;

                case ModSetting.CombatReaction:
                {
                    CombatReaction = p_state;
                    CombatReactionChange?.Invoke(p_state);
                } break;

                case ModSetting.AutoRecover:
                {
                    AutoRecover = p_state;
                    AutoRecoverChange?.Invoke(p_state);
                } break;

                case ModSetting.Slipperiness:
                {
                    Slipperiness = p_state;
                    SlipperinessChange?.Invoke(p_state);
                } break;

                case ModSetting.Bounciness:
                {
                    Bounciness = p_state;
                    BouncinessChange?.Invoke(p_state);
                } break;

                case ModSetting.ViewVelocity:
                {
                    ViewVelocity = p_state;
                    ViewVelocityChange?.Invoke(p_state);
                } break;

                case ModSetting.VrFollow:
                {
                    VrFollow = p_state;
                    VrFollowChange?.Invoke(p_state);
                } break;
            }

            ms_entries[(int)p_setting].BoxedValue = p_state;
            if(p_uiIndex != UiElementIndex.Count)
                (ms_uiElements[(int)p_uiIndex] as BTKUILib.UIObjects.Components.ToggleButton).ToggleValue = p_state;
        }

        static void OnSliderUpdate(ModSetting p_setting, float p_value, UiElementIndex p_uiIndex = UiElementIndex.Count)
        {
            switch(p_setting)
            {
                case ModSetting.VelocityMultiplier:
                {
                    VelocityMultiplier = p_value;
                    VelocityMultiplierChange?.Invoke(p_value);
                } break;

                case ModSetting.MovementDrag:
                {
                    MovementDrag = p_value;
                    MovementDragChange?.Invoke(p_value);
                } break;

                case ModSetting.AngularDrag:
                {
                    AngularDrag = p_value;
                    AngularDragChange?.Invoke(p_value);
                } break;

                case ModSetting.RecoverDelay:
                {
                    RecoverDelay = p_value;
                    RecoverDelayChange?.Invoke(p_value);
                } break;
            }

            ms_entries[(int)p_setting].BoxedValue = p_value;
            if(p_uiIndex != UiElementIndex.Count)
                (ms_uiElements[(int)p_uiIndex] as BTKUILib.UIObjects.Components.SliderFloat).SetSliderValue(p_value);
        }

        static void Reset()
        {
            OnToggleUpdate(ModSetting.Hotkey, true, UiElementIndex.Hotkey);
            OnToggleUpdate(ModSetting.RestorePosition, false, UiElementIndex.RestorePosition);
            OnToggleUpdate(ModSetting.Gravity, true, UiElementIndex.Gravity);
            OnToggleUpdate(ModSetting.PointersReaction, true, UiElementIndex.PointersReaction);
            OnToggleUpdate(ModSetting.IgnoreLocal, true, UiElementIndex.IgnoreLocal);
            OnToggleUpdate(ModSetting.CombatReaction, true, UiElementIndex.CombatReaction);
            OnToggleUpdate(ModSetting.AutoRecover, false, UiElementIndex.AutoRecover);
            OnToggleUpdate(ModSetting.Slipperiness, false, UiElementIndex.Slipperiness);
            OnToggleUpdate(ModSetting.Bounciness, false, UiElementIndex.Bounciness);
            OnToggleUpdate(ModSetting.ViewVelocity, false, UiElementIndex.ViewVelocity);
            OnToggleUpdate(ModSetting.VrFollow, true, UiElementIndex.VrFollow);
            OnSliderUpdate(ModSetting.VelocityMultiplier, 2f, UiElementIndex.VelocityMultiplier);
            OnSliderUpdate(ModSetting.MovementDrag, 2f, UiElementIndex.MovementDrag);
            OnSliderUpdate(ModSetting.AngularDrag, 2f, UiElementIndex.AngularDrag);
            OnSliderUpdate(ModSetting.RecoverDelay, 3f, UiElementIndex.RecoverDelay);
        }
    }
}

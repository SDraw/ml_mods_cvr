using System;
using System.IO;
using System.Reflection;
using BTKUILib.UIObjects;
using BTKUILib.UIObjects.Components;

namespace ml_prm
{
    static class ModUi
    {
        internal class UiEvent
        {
            event Action m_action;
            public void AddHandler(Action p_listener) => m_action += p_listener;
            public void RemoveHandler(Action p_listener) => m_action -= p_listener;
            public void Invoke() => m_action?.Invoke();
        }

        enum UiIndex
        {
            Hotkey = 0,
            Gravity,
            PointersReaction,
            IgnoreLocal,
            CombatReaction,
            AutoRecover,
            Slipperiness,
            Bounciness,
            ViewVelocity,
            JumpRecover,
            Buoyancy,
            FallDamage,
            VelocityMultiplier,
            MovementDrag,
            AngularDrag,
            RecoverDelay,
            FallLimit,
            GestureGrab,
            FriendsGrab,
            GrabDistance
        }

        const string c_ragdollKeyTooltip = "Switch ragdoll mode with '{0}' key";
        const string c_fallLimitTooltip = "Fall limit based on impact velocity<p>Current value corresponds to drop from {0} units with default gravity</p>";

        internal static readonly UiEvent OnSwitchChanged = new UiEvent();

        static Page ms_page = null;
        static Category ms_category = null;

        static Button ms_ragdollButton = null;
        static ToggleButton ms_hotkeyToggle = null;
        static ToggleButton ms_gravityToggle = null;
        static ToggleButton ms_pointersToggle = null;
        static ToggleButton ms_ignoreLocalToggle = null;
        static ToggleButton ms_combatToggle = null;
        static ToggleButton ms_recoveryToggle = null;
        static ToggleButton ms_slipperinessToggle = null;
        static ToggleButton ms_bouncinessToggle = null;
        static ToggleButton ms_viewDirectionToggle = null;
        static ToggleButton ms_jumpRecoverToggle = null;
        static ToggleButton ms_buoyancyToggle = null;
        static ToggleButton ms_fallDamageToggle = null;
        static ToggleButton ms_gestureGrabToggle = null;
        static ToggleButton ms_friendsGrabToggle = null;
        static SliderFloat ms_velocityMultiplierSlider = null;
        static SliderFloat ms_movementDragSlider = null;
        static SliderFloat ms_angularMovementDragSlider = null;
        static SliderFloat ms_recoverDelaySlider = null;
        static SliderFloat ms_fallLimitSlider = null;
        static SliderFloat ms_grabDistanceSlider = null;
        static Button ms_resetButton = null;

        internal static void Init()
        {
            BTKUILib.QuickMenuAPI.PrepareIcon("PlayerRagdollMod", "PRM-Person", GetIconStream("person.png"));

            ms_page = new Page("PlayerRagdollMod", "MainPage", true, "PRM-Person");
            ms_page.MenuTitle = "Player Ragdoll Mod";
            ms_page.MenuSubtitle = "Become a ragdoll and change various settings for people amusement";

            ms_category = ms_page.AddCategory("Settings");

            ms_ragdollButton = ms_category.AddButton("Switch ragdoll", "PRM-Person", "Switch between normal and ragdoll state");
            ms_ragdollButton.OnPress += OnSwitch;

            ms_hotkeyToggle = ms_category.AddToggle("Use hotkey", "Switch ragdoll mode with 'R' key", Settings.Hotkey);
            ms_hotkeyToggle.ToggleTooltip = string.Format(c_ragdollKeyTooltip, Settings.HotkeyKey);
            ms_hotkeyToggle.OnValueUpdated += (state) => OnToggleUpdate(UiIndex.Hotkey, state);
            Settings.OnHotkeyKeyChanged.AddHandler(OnHotkeyKeyChanged);

            ms_gravityToggle = ms_category.AddToggle("Use gravity", "Apply gravity to ragdoll", Settings.Gravity);
            ms_gravityToggle.OnValueUpdated += (state) => OnToggleUpdate(UiIndex.Gravity, state);

            ms_pointersToggle = ms_category.AddToggle("Pointers reaction", "React to trigger colliders with CVRPointer component of 'ragdoll' type", Settings.PointersReaction);
            ms_pointersToggle.OnValueUpdated += (state) => OnToggleUpdate(UiIndex.PointersReaction, state);

            ms_ignoreLocalToggle = ms_category.AddToggle("Ignore local pointers", "Ignore local avatar's CVRPointer components of 'ragdoll' type", Settings.IgnoreLocal);
            ms_ignoreLocalToggle.OnValueUpdated += (state) => OnToggleUpdate(UiIndex.IgnoreLocal, state);

            ms_combatToggle = ms_category.AddToggle("Combat reaction", "Ragdoll upon combat system death", Settings.CombatReaction);
            ms_combatToggle.OnValueUpdated += (state) => OnToggleUpdate(UiIndex.CombatReaction, state);

            ms_recoveryToggle = ms_category.AddToggle("Auto recover", "Automatically unragdoll after set recover delay", Settings.AutoRecover);
            ms_recoveryToggle.OnValueUpdated += (state) => OnToggleUpdate(UiIndex.AutoRecover, state);

            ms_slipperinessToggle = ms_category.AddToggle("Slipperiness", "Enables/disables friction of ragdoll", Settings.Slipperiness);
            ms_slipperinessToggle.OnValueUpdated += (state) => OnToggleUpdate(UiIndex.Slipperiness, state);

            ms_bouncinessToggle = ms_category.AddToggle("Bounciness", "Enables/disables bounciness of ragdoll", Settings.Bounciness);
            ms_bouncinessToggle.OnValueUpdated += (state) => OnToggleUpdate(UiIndex.Bounciness, state);

            ms_viewDirectionToggle = ms_category.AddToggle("View direction velocity", "Apply velocity to camera view direction", Settings.ViewVelocity);
            ms_viewDirectionToggle.OnValueUpdated += (state) => OnToggleUpdate(UiIndex.ViewVelocity, state);

            ms_jumpRecoverToggle = ms_category.AddToggle("Jump recover", "Recover from ragdoll state by jumping", Settings.JumpRecover);
            ms_jumpRecoverToggle.OnValueUpdated += (state) => OnToggleUpdate(UiIndex.JumpRecover, state);

            ms_buoyancyToggle = ms_category.AddToggle("Buoyancy", "Enable buoyancy in fluid volumes<p>Warning: constantly changes movement and air drag of hips, spine and chest</p>", Settings.Buoyancy);
            ms_buoyancyToggle.OnValueUpdated += (state) => OnToggleUpdate(UiIndex.Buoyancy, state);

            ms_fallDamageToggle = ms_category.AddToggle("Fall damage", "Enable ragdoll when falling from height", Settings.FallDamage);
            ms_fallDamageToggle.OnValueUpdated += (state) => OnToggleUpdate(UiIndex.FallDamage, state);

            ms_gestureGrabToggle = ms_category.AddToggle("Gesture grab", "Enable grabbing of ragdolled body parts by remote players with trigger/grab gesture<p>Warning: can lead to unpredictable physics behaviour in some cases", Settings.GestureGrab);
            ms_gestureGrabToggle.OnValueUpdated += (state) => OnToggleUpdate(UiIndex.GestureGrab, state);

            ms_friendsGrabToggle = ms_category.AddToggle("Friends grab only", " ", Settings.FriendsGrab);
            ms_friendsGrabToggle.OnValueUpdated += (state) => OnToggleUpdate(UiIndex.FriendsGrab, state);

            ms_velocityMultiplierSlider = ms_category.AddSlider("Velocity multiplier", "Velocity multiplier upon entering ragdoll state", Settings.VelocityMultiplier, 1f, 50f);
            ms_velocityMultiplierSlider.OnValueUpdated += (value) => OnSliderUpdate(UiIndex.VelocityMultiplier, value);

            ms_movementDragSlider = ms_category.AddSlider("Movement drag", "Movement resistance", Settings.MovementDrag, 0f, 50f);
            ms_movementDragSlider.OnValueUpdated += (value) => OnSliderUpdate(UiIndex.MovementDrag, value);

            ms_angularMovementDragSlider = ms_category.AddSlider("Angular movement drag", "Rotation movement resistance", Settings.AngularDrag, 0f, 50f);
            ms_angularMovementDragSlider.OnValueUpdated += (value) => OnSliderUpdate(UiIndex.AngularDrag, value);

            ms_recoverDelaySlider = ms_category.AddSlider("Recover delay (seconds)", "Recover delay for automatic recover", Settings.RecoverDelay, 1f, 10f);
            ms_recoverDelaySlider.OnValueUpdated += (value) => OnSliderUpdate(UiIndex.RecoverDelay, value);

            ms_fallLimitSlider = ms_category.AddSlider("Fall limit", "", Settings.FallLimit, 4.5f, 44.5f);
            ms_fallLimitSlider.SliderTooltip = string.Format(c_fallLimitTooltip, GetDropHeight(Settings.FallLimit));
            ms_fallLimitSlider.OnValueUpdated += (value) => OnSliderUpdate(UiIndex.FallLimit, value);

            ms_grabDistanceSlider = ms_category.AddSlider("Grab distance", "Minimal distance for successful grab", Settings.GrabDistance, 0f, 1f);
            ms_grabDistanceSlider.OnValueUpdated += (value) => OnSliderUpdate(UiIndex.GrabDistance, value);

            ms_resetButton = ms_category.AddButton("Reset settings", "", "Reset mod settings to default");
            ms_resetButton.OnPress += Reset;
        }

        static void OnSwitch()
        {
            try
            {
                OnSwitchChanged.Invoke();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnToggleUpdate(UiIndex p_index, bool p_state)
        {
            try
            {
                switch(p_index)
                {
                    case UiIndex.Hotkey:
                        Settings.SetSetting(Settings.ModSetting.Hotkey, p_state);
                        break;

                    case UiIndex.Gravity:
                        Settings.SetSetting(Settings.ModSetting.Gravity, p_state);
                        break;

                    case UiIndex.PointersReaction:
                        Settings.SetSetting(Settings.ModSetting.PointersReaction, p_state);
                        break;

                    case UiIndex.IgnoreLocal:
                        Settings.SetSetting(Settings.ModSetting.IgnoreLocal, p_state);
                        break;

                    case UiIndex.CombatReaction:
                        Settings.SetSetting(Settings.ModSetting.CombatReaction, p_state);
                        break;

                    case UiIndex.AutoRecover:
                        Settings.SetSetting(Settings.ModSetting.AutoRecover, p_state);
                        break;

                    case UiIndex.Slipperiness:
                        Settings.SetSetting(Settings.ModSetting.Slipperiness, p_state);
                        break;

                    case UiIndex.Bounciness:
                        Settings.SetSetting(Settings.ModSetting.Bounciness, p_state);
                        break;

                    case UiIndex.ViewVelocity:
                        Settings.SetSetting(Settings.ModSetting.ViewVelocity, p_state);
                        break;

                    case UiIndex.JumpRecover:
                        Settings.SetSetting(Settings.ModSetting.JumpRecover, p_state);
                        break;

                    case UiIndex.Buoyancy:
                        Settings.SetSetting(Settings.ModSetting.Buoyancy, p_state);
                        break;

                    case UiIndex.FallDamage:
                        Settings.SetSetting(Settings.ModSetting.FallDamage, p_state);
                        break;

                    case UiIndex.GestureGrab:
                        Settings.SetSetting(Settings.ModSetting.GestureGrab, p_state);
                        break;

                    case UiIndex.FriendsGrab:
                        Settings.SetSetting(Settings.ModSetting.FriendsGrab, p_state);
                        break;
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnSliderUpdate(UiIndex p_index, float p_value)
        {
            try
            {
                switch(p_index)
                {
                    case UiIndex.VelocityMultiplier:
                        Settings.SetSetting(Settings.ModSetting.VelocityMultiplier, p_value);
                        break;

                    case UiIndex.MovementDrag:
                        Settings.SetSetting(Settings.ModSetting.MovementDrag, p_value);
                        break;

                    case UiIndex.AngularDrag:
                        Settings.SetSetting(Settings.ModSetting.AngularDrag, p_value);
                        break;

                    case UiIndex.RecoverDelay:
                        Settings.SetSetting(Settings.ModSetting.RecoverDelay, p_value);
                        break;

                    case UiIndex.FallLimit:
                    {
                        Settings.SetSetting(Settings.ModSetting.FallLimit, p_value);
                        ms_fallLimitSlider.SliderTooltip = string.Format(c_fallLimitTooltip, GetDropHeight(p_value));
                    }
                    break;

                    case UiIndex.GrabDistance:
                        Settings.SetSetting(Settings.ModSetting.GrabDistance, p_value);
                        break;
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void Reset()
        {
            OnToggleUpdate(UiIndex.Hotkey, true);
            ms_hotkeyToggle.ToggleValue = true;

            OnToggleUpdate(UiIndex.Gravity, true);
            ms_gravityToggle.ToggleValue = true;

            OnToggleUpdate(UiIndex.PointersReaction, true);
            ms_pointersToggle.ToggleValue = true;

            OnToggleUpdate(UiIndex.IgnoreLocal, true);
            ms_ignoreLocalToggle.ToggleValue = true;

            OnToggleUpdate(UiIndex.CombatReaction, true);
            ms_combatToggle.ToggleValue = true;

            OnToggleUpdate(UiIndex.AutoRecover, false);
            ms_recoveryToggle.ToggleValue = false;

            OnToggleUpdate(UiIndex.Slipperiness, false);
            ms_slipperinessToggle.ToggleValue = false;

            OnToggleUpdate(UiIndex.Bounciness, false);
            ms_bouncinessToggle.ToggleValue = false;

            OnToggleUpdate(UiIndex.ViewVelocity, false);
            ms_viewDirectionToggle.ToggleValue = false;

            OnToggleUpdate(UiIndex.JumpRecover, false);
            ms_jumpRecoverToggle.ToggleValue = false;

            OnToggleUpdate(UiIndex.Buoyancy, true);
            ms_buoyancyToggle.ToggleValue = true;

            OnToggleUpdate(UiIndex.FallDamage, true);
            ms_fallDamageToggle.ToggleValue = true;

            OnToggleUpdate(UiIndex.GestureGrab, false);
            ms_gestureGrabToggle.ToggleValue = false;

            OnToggleUpdate(UiIndex.FriendsGrab, true);
            ms_friendsGrabToggle.ToggleValue = true;

            OnSliderUpdate(UiIndex.VelocityMultiplier, 2f);
            ms_velocityMultiplierSlider.SetSliderValue(2f);

            OnSliderUpdate(UiIndex.MovementDrag, 1f);
            ms_movementDragSlider.SetSliderValue(1f);

            OnSliderUpdate(UiIndex.AngularDrag, 1f);
            ms_angularMovementDragSlider.SetSliderValue(1f);

            OnSliderUpdate(UiIndex.RecoverDelay, 3f);
            ms_recoverDelaySlider.SetSliderValue(3f);

            OnSliderUpdate(UiIndex.FallLimit, 9.899494f);
            ms_fallLimitSlider.SetSliderValue(9.899494f);

            OnSliderUpdate(UiIndex.GrabDistance, 0.1f);
            ms_grabDistanceSlider.SetSliderValue(0.1f);
        }

        static void OnHotkeyKeyChanged(UnityEngine.KeyCode p_keyCode)
        {
            if(ms_ragdollButton != null)
                ms_hotkeyToggle.ToggleTooltip = string.Format(c_ragdollKeyTooltip, p_keyCode);
        }

        static Stream GetIconStream(string p_name)
        {
            Assembly l_assembly = Assembly.GetExecutingAssembly();
            string l_assemblyName = l_assembly.GetName().Name;
            return l_assembly.GetManifestResourceStream(l_assemblyName + ".resources." + p_name);
        }

        static float GetDropHeight(float p_speed, float p_gravity = 9.8f)
        {
            return MathF.Pow(p_speed, 2f) / (p_gravity * 2f);
        }
    }
}

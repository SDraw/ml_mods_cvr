using System;
using System.Collections.Generic;
using UnityEngine;

namespace ml_prm
{
    static class Settings
    {
        internal class SettingEvent<T>
        {
            event Action<T> m_action;
            public void AddListener(Action<T> p_listener) => m_action += p_listener;
            public void RemoveListener(Action<T> p_listener) => m_action -= p_listener;
            public void Invoke(T p_value) => m_action?.Invoke(p_value);
        }

        public enum ModSetting
        {
            Hotkey = 0,
            HotkeyKey,
            VelocityMultiplier,
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
            JumpRecover,
            Buoyancy,
            FallDamage,
            FallLimit,
            GestureGrab,
            FriendsGrab,
            GrabDistance
        }

        public static bool Hotkey { get; private set; } = true;
        public static KeyCode HotkeyKey { get; private set; } = KeyCode.R;
        public static float VelocityMultiplier { get; private set; } = 2f;
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
        public static bool JumpRecover { get; private set; } = false;
        public static bool Buoyancy { get; private set; } = true;
        public static bool FallDamage { get; private set; } = true;
        public static float FallLimit { get; private set; } = 9.899494f;
        public static bool GestureGrab { get; private set; } = false;
        public static bool FriendsGrab { get; private set; } = true;
        public static float GrabDistance { get; private set; } = 0.1f;

        public static readonly SettingEvent<bool> OnHotkeyChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<KeyCode> OnHotkeyKeyChanged = new SettingEvent<KeyCode>();
        public static readonly SettingEvent<float> OnVelocityMultiplierChanged = new SettingEvent<float>();
        public static readonly SettingEvent<float> OnMovementDragChanged = new SettingEvent<float>();
        public static readonly SettingEvent<float> OnAngularDragChanged = new SettingEvent<float>();
        public static readonly SettingEvent<bool> OnGravityChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnPointersReactionChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnIgnoreLocalChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnCombatReactionChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnAutoRecoverChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<float> OnRecoverDelayChanged = new SettingEvent<float>();
        public static readonly SettingEvent<bool> OnSlipperinessChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnBouncinessChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnViewVelocityChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnJumpRecoverChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnBuoyancyChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnFallDamageChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<float> OnFallLimitChanged = new SettingEvent<float>();
        public static readonly SettingEvent<bool> OnGestureGrabChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnFriendsGrabChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<float> OnGrabDistanceChanged = new SettingEvent<float>();

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("PRM", "Player Ragdoll Mod");

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.Hotkey.ToString(), Hotkey, null, null, true),
                ms_category.CreateEntry(ModSetting.HotkeyKey.ToString(), HotkeyKey, "Hotkey"),
                ms_category.CreateEntry(ModSetting.VelocityMultiplier.ToString(), VelocityMultiplier, null, null, true),
                ms_category.CreateEntry(ModSetting.MovementDrag.ToString(), MovementDrag, null, null, true),
                ms_category.CreateEntry(ModSetting.AngularDrag.ToString(), AngularDrag, null, null, true),
                ms_category.CreateEntry(ModSetting.Gravity.ToString(), Gravity, null, null, true),
                ms_category.CreateEntry(ModSetting.PointersReaction.ToString(), PointersReaction, null, null, true),
                ms_category.CreateEntry(ModSetting.IgnoreLocal.ToString(), IgnoreLocal, null, null, true),
                ms_category.CreateEntry(ModSetting.CombatReaction.ToString(), CombatReaction, null, null, true),
                ms_category.CreateEntry(ModSetting.AutoRecover.ToString(), AutoRecover, null, null, true),
                ms_category.CreateEntry(ModSetting.RecoverDelay.ToString(), RecoverDelay, null, null, true),
                ms_category.CreateEntry(ModSetting.Slipperiness.ToString(), Slipperiness, null, null, true),
                ms_category.CreateEntry(ModSetting.Bounciness.ToString(), Bounciness, null, null, true),
                ms_category.CreateEntry(ModSetting.ViewVelocity.ToString(), ViewVelocity, null, null, true),
                ms_category.CreateEntry(ModSetting.JumpRecover.ToString(), JumpRecover, null, null, true),
                ms_category.CreateEntry(ModSetting.Buoyancy.ToString(), Buoyancy, null, null, true),
                ms_category.CreateEntry(ModSetting.FallDamage.ToString(), FallDamage, null, null, true),
                ms_category.CreateEntry(ModSetting.FallLimit.ToString(), FallLimit, null, null, true),
                ms_category.CreateEntry(ModSetting.GestureGrab.ToString(), GestureGrab, null, null, true),
                ms_category.CreateEntry(ModSetting.FriendsGrab.ToString(), FriendsGrab, null, null, true),
                ms_category.CreateEntry(ModSetting.GrabDistance.ToString(), GrabDistance, null, null, true),
            };

            ms_entries[(int)ModSetting.HotkeyKey].OnEntryValueChangedUntyped.Subscribe(OnMelonSettingSave_HotkeyKey);

            Hotkey = (bool)ms_entries[(int)ModSetting.Hotkey].BoxedValue;
            HotkeyKey = (KeyCode)ms_entries[(int)ModSetting.HotkeyKey].BoxedValue;
            VelocityMultiplier = Mathf.Clamp((float)ms_entries[(int)ModSetting.VelocityMultiplier].BoxedValue, 1f, 50f);
            MovementDrag = Mathf.Clamp((float)ms_entries[(int)ModSetting.MovementDrag].BoxedValue, 0f, 50f);
            AngularDrag = Mathf.Clamp((float)ms_entries[(int)ModSetting.AngularDrag].BoxedValue, 0f, 50f);
            Gravity = (bool)ms_entries[(int)ModSetting.Gravity].BoxedValue;
            PointersReaction = (bool)ms_entries[(int)ModSetting.PointersReaction].BoxedValue;
            IgnoreLocal = (bool)ms_entries[(int)ModSetting.IgnoreLocal].BoxedValue;
            CombatReaction = (bool)ms_entries[(int)ModSetting.CombatReaction].BoxedValue;
            AutoRecover = (bool)ms_entries[(int)ModSetting.AutoRecover].BoxedValue;
            RecoverDelay = Mathf.Clamp((float)ms_entries[(int)ModSetting.RecoverDelay].BoxedValue, 1f, 10f);
            Slipperiness = (bool)ms_entries[(int)ModSetting.Slipperiness].BoxedValue;
            Bounciness = (bool)ms_entries[(int)ModSetting.Bounciness].BoxedValue;
            ViewVelocity = (bool)ms_entries[(int)ModSetting.ViewVelocity].BoxedValue;
            JumpRecover = (bool)ms_entries[(int)ModSetting.JumpRecover].BoxedValue;
            Buoyancy = (bool)ms_entries[(int)ModSetting.Buoyancy].BoxedValue;
            FallDamage = (bool)ms_entries[(int)ModSetting.FallDamage].BoxedValue;
            FallLimit = Mathf.Clamp((float)ms_entries[(int)ModSetting.FallLimit].BoxedValue, 4.5f, 44.5f);
            GestureGrab = (bool)ms_entries[(int)ModSetting.GestureGrab].BoxedValue;
            FriendsGrab = (bool)ms_entries[(int)ModSetting.FriendsGrab].BoxedValue;
            GrabDistance = Mathf.Clamp01((float)ms_entries[(int)ModSetting.GrabDistance].BoxedValue);
        }

        static void OnMelonSettingSave_HotkeyKey(object p_oldValue, object p_newValue)
        {
            try
            {
                if(p_newValue is KeyCode code)
                {
                    HotkeyKey = code;
                    OnHotkeyKeyChanged.Invoke(HotkeyKey);
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        public static void SetSetting(ModSetting p_settings, object p_value)
        {
            try
            {
                switch(p_settings)
                {
                    // Booleans
                    case ModSetting.Hotkey:
                    {
                        Hotkey = (bool)p_value;
                        OnHotkeyChanged.Invoke(Hotkey);
                    }
                    break;

                    case ModSetting.Gravity:
                    {
                        Gravity = (bool)p_value;
                        OnGravityChanged.Invoke(Gravity);
                    }
                    break;

                    case ModSetting.PointersReaction:
                    {
                        PointersReaction = (bool)p_value;
                        OnPointersReactionChanged.Invoke(PointersReaction);
                    }
                    break;

                    case ModSetting.IgnoreLocal:
                    {
                        IgnoreLocal = (bool)p_value;
                        OnIgnoreLocalChanged.Invoke(IgnoreLocal);
                    }
                    break;

                    case ModSetting.CombatReaction:
                    {
                        CombatReaction = (bool)p_value;
                        OnCombatReactionChanged.Invoke(CombatReaction);
                    }
                    break;

                    case ModSetting.AutoRecover:
                    {
                        AutoRecover = (bool)p_value;
                        OnAutoRecoverChanged.Invoke(AutoRecover);
                    }
                    break;

                    case ModSetting.Slipperiness:
                    {
                        Slipperiness = (bool)p_value;
                        OnSlipperinessChanged.Invoke(Slipperiness);
                    }
                    break;

                    case ModSetting.Bounciness:
                    {
                        Bounciness = (bool)p_value;
                        OnBouncinessChanged.Invoke(Bounciness);
                    }
                    break;

                    case ModSetting.ViewVelocity:
                    {
                        ViewVelocity = (bool)p_value;
                        OnViewVelocityChanged.Invoke(ViewVelocity);
                    }
                    break;

                    case ModSetting.JumpRecover:
                    {
                        JumpRecover = (bool)p_value;
                        OnJumpRecoverChanged.Invoke(JumpRecover);
                    }
                    break;

                    case ModSetting.Buoyancy:
                    {
                        Buoyancy = (bool)p_value;
                        OnBuoyancyChanged.Invoke(Buoyancy);
                    }
                    break;

                    case ModSetting.FallDamage:
                    {
                        FallDamage = (bool)p_value;
                        OnFallDamageChanged.Invoke(FallDamage);
                    }
                    break;

                    case ModSetting.GestureGrab:
                    {
                        GestureGrab = (bool)p_value;
                        OnGestureGrabChanged.Invoke(GestureGrab);
                    }
                    break;

                    case ModSetting.FriendsGrab:
                    {
                        FriendsGrab = (bool)p_value;
                        OnFriendsGrabChanged.Invoke(FriendsGrab);
                    }
                    break;

                    // Floats
                    case ModSetting.VelocityMultiplier:
                    {
                        VelocityMultiplier = (float)p_value;
                        OnVelocityMultiplierChanged.Invoke(VelocityMultiplier);
                    }
                    break;

                    case ModSetting.MovementDrag:
                    {
                        MovementDrag = (float)p_value;
                        OnMovementDragChanged.Invoke(MovementDrag);
                    }
                    break;

                    case ModSetting.AngularDrag:
                    {
                        AngularDrag = (float)p_value;
                        OnAngularDragChanged.Invoke(AngularDrag);
                    }
                    break;

                    case ModSetting.RecoverDelay:
                    {
                        RecoverDelay = (float)p_value;
                        OnRecoverDelayChanged.Invoke(RecoverDelay);
                    }
                    break;

                    case ModSetting.FallLimit:
                    {
                        FallLimit = (float)p_value;
                        OnFallLimitChanged.Invoke(FallLimit);
                    }
                    break;

                    case ModSetting.GrabDistance:
                    {
                        GrabDistance = (float)p_value;
                        OnGrabDistanceChanged.Invoke(GrabDistance);
                    }
                    break;
                }

                if(ms_entries != null)
                    ms_entries[(int)p_settings].BoxedValue = p_value;
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

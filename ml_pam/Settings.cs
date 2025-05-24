using ABI_RC.Core.InteractionSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ml_pam
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
            Enabled = 0,
            GrabOffset,
            LeadHand,
            HandsExtension,
            ExtensionSpeed,
            LeftHandKey,
            RightHandKey
        }
        public enum LeadHand
        {
            Left = 0,
            Right,
            Both
        }

        public static bool Enabled { get; private set; } = true;
        public static float GrabOffset { get; private set; } = 0.5f;
        public static LeadHand LeadingHand { get; private set; } = LeadHand.Right;
        public static bool HandsExtension { get; private set; } = true;
        public static float ExtensionSpeed { get; private set; } = 10f;
        public static KeyCode LeftHandKey { get; private set; } = KeyCode.Q;
        public static KeyCode RightHandKey { get; private set; } = KeyCode.E;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        public static readonly SettingEvent<bool> OnEnabledChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<float> OnGrabOffsetChanged = new SettingEvent<float>();
        public static readonly SettingEvent<LeadHand> OnLeadingHandChanged = new SettingEvent<LeadHand>();
        public static readonly SettingEvent<bool> OnHandsExtensionChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<float> OnExtensionSpeedChanged = new SettingEvent<float>();
        public static readonly SettingEvent<KeyCode> OnLeftHandKeyChanged = new SettingEvent<KeyCode>();
        public static readonly SettingEvent<KeyCode> OnRightHandKeyChanged = new SettingEvent<KeyCode>();

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("PAM", "Pickup Arm Movement");

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.Enabled.ToString(), Enabled, null, null, true),
                ms_category.CreateEntry(ModSetting.GrabOffset.ToString(), (int)(GrabOffset * 100f), null, null, true),
                ms_category.CreateEntry(ModSetting.LeadHand.ToString(), (int)LeadHand.Right, null, null, true),
                ms_category.CreateEntry(ModSetting.HandsExtension.ToString(), HandsExtension, null, null, true),
                ms_category.CreateEntry(ModSetting.ExtensionSpeed.ToString(), (int)ExtensionSpeed, null, null, true),
                ms_category.CreateEntry(ModSetting.LeftHandKey.ToString(), LeftHandKey, "Left hand key"),
                ms_category.CreateEntry(ModSetting.RightHandKey.ToString(), RightHandKey, "Right hand key")
            };

            ms_entries[(int)ModSetting.LeftHandKey].OnEntryValueChangedUntyped.Subscribe(OnMelonSettingSave_LeftHandKey);
            ms_entries[(int)ModSetting.RightHandKey].OnEntryValueChangedUntyped.Subscribe(OnMelonSettingSave_RightHandKey);

            Enabled = (bool)ms_entries[(int)ModSetting.Enabled].BoxedValue;
            GrabOffset = (int)ms_entries[(int)ModSetting.GrabOffset].BoxedValue * 0.01f;
            LeadingHand = (LeadHand)(int)ms_entries[(int)ModSetting.LeadHand].BoxedValue;
            HandsExtension = (bool)ms_entries[(int)ModSetting.HandsExtension].BoxedValue;
            ExtensionSpeed = Math.Clamp((int)ms_entries[(int)ModSetting.ExtensionSpeed].BoxedValue, 1f, 50f);
            LeftHandKey = (KeyCode)ms_entries[(int)ModSetting.LeftHandKey].BoxedValue;
            RightHandKey = (KeyCode)ms_entries[(int)ModSetting.RightHandKey].BoxedValue;

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
                MelonLoader.MelonCoroutines.Start(UpdateMenuSettings());
            };
        }

        static System.Collections.IEnumerator UpdateMenuSettings()
        {
            while(!ViewManager.Instance.IsReady || !ViewManager.Instance.IsMainMenuOpen)
                yield return null;

            foreach(var l_entry in ms_entries)
                ViewManager.Instance.gameMenuView.View.TriggerEvent("updateModSetting", ms_category.Identifier, l_entry.DisplayName, l_entry.GetValueAsString());
        }

        static void OnMelonSettingSave_LeftHandKey(object p_oldValue, object p_newValue)
        {
            try
            {
                if(p_newValue is KeyCode code)
                {
                    LeftHandKey = code;
                    OnLeftHandKeyChanged.Invoke(LeftHandKey);
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnMelonSettingSave_RightHandKey(object p_oldValue, object p_newValue)
        {
            try
            {
                if(p_newValue is KeyCode code)
                {
                    RightHandKey = code;
                    OnRightHandKeyChanged.Invoke(RightHandKey);
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnToggleUpdate(string p_name, string p_value)
        {
            try
            {
                if(Enum.TryParse(p_name, out ModSetting l_setting) && bool.TryParse(p_value, out bool l_value))
                {
                    switch(l_setting)
                    {
                        case ModSetting.Enabled:
                        {
                            Enabled = l_value;
                            OnEnabledChanged.Invoke(Enabled);
                        }
                        break;

                        case ModSetting.HandsExtension:
                        {
                            HandsExtension = l_value;
                            OnHandsExtensionChanged.Invoke(HandsExtension);
                        }
                        break;
                    }

                    ms_entries[(int)l_setting].BoxedValue = l_value;
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnSliderUpdate(string p_name, string p_value)
        {
            try
            {
                if(Enum.TryParse(p_name, out ModSetting l_setting) && int.TryParse(p_value, out int l_value))
                {
                    switch(l_setting)
                    {
                        case ModSetting.GrabOffset:
                        {
                            GrabOffset = l_value * 0.01f;
                            OnGrabOffsetChanged.Invoke(GrabOffset);
                        }
                        break;

                        case ModSetting.ExtensionSpeed:
                        {
                            ExtensionSpeed = l_value;
                            OnExtensionSpeedChanged.Invoke(ExtensionSpeed);
                        }
                        break;
                    }

                    ms_entries[(int)l_setting].BoxedValue = l_value;
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnDropdownUpdate(string p_name, string p_value)
        {
            try
            {
                if(Enum.TryParse(p_name, out ModSetting l_setting) && int.TryParse(p_value, out int l_value))
                {
                    switch(l_setting)
                    {
                        case ModSetting.LeadHand:
                        {
                            LeadingHand = (LeadHand)l_value;
                            OnLeadingHandChanged.Invoke(LeadingHand);
                        }
                        break;
                    }

                    ms_entries[(int)l_setting].BoxedValue = l_value;
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

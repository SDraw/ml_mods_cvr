using ABI_RC.Core.InteractionSystem;
using System;
using System.Collections.Generic;

namespace ml_pin
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

        public enum NotificationType
        {
            None = 0,
            Friends,
            All
        };

        enum ModSetting
        {
            NotifyType,
            Volume,
            Delay,
            NotifyInPublic,
            NotifyInFriends,
            NotifyInPrivate,
            FriendsAlways
        };

        public static NotificationType NotifyType { get; private set; } = NotificationType.All;
        public static float Volume { get; private set; } = 1.0f;
        public static bool Delay { get; private set; } = true;
        public static bool NotifyInPublic { get; private set; } = true;
        public static bool NotifyInFriends { get; private set; } = true;
        public static bool NotifyInPrivate { get; private set; } = true;
        public static bool FriendsAlways { get; private set; } = false;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        public static readonly SettingEvent<NotificationType> OnNotifyTypeChanged = new SettingEvent<NotificationType>();
        public static readonly SettingEvent<float> OnVolumeChanged = new SettingEvent<float>();
        public static readonly SettingEvent<bool> OnDelayChange = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnNotifyInPublicChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnNotifyInFriendsChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnNotifyInPrivateChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnFriendsAlwaysChanged = new SettingEvent<bool>();

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("PIN", null, true);

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.NotifyType.ToString(), (int)NotifyType),
                ms_category.CreateEntry(ModSetting.Volume.ToString(), (int)(Volume * 100f)),
                ms_category.CreateEntry(ModSetting.Delay.ToString(), Delay),
                ms_category.CreateEntry(ModSetting.NotifyInPublic.ToString(), NotifyInPublic),
                ms_category.CreateEntry(ModSetting.NotifyInFriends.ToString(), NotifyInFriends),
                ms_category.CreateEntry(ModSetting.NotifyInPrivate.ToString(), NotifyInPrivate),
                ms_category.CreateEntry(ModSetting.FriendsAlways.ToString(), FriendsAlways),
            };

            NotifyType = (NotificationType)(int)ms_entries[(int)ModSetting.NotifyType].BoxedValue;
            Volume = (int)ms_entries[(int)ModSetting.Volume].BoxedValue * 0.01f;
            Delay = (bool)ms_entries[(int)ModSetting.Delay].BoxedValue;
            NotifyInPublic = (bool)ms_entries[(int)ModSetting.NotifyInPublic].BoxedValue;
            NotifyInFriends = (bool)ms_entries[(int)ModSetting.NotifyInFriends].BoxedValue;
            NotifyInPrivate = (bool)ms_entries[(int)ModSetting.NotifyInPrivate].BoxedValue;
            FriendsAlways = (bool)ms_entries[(int)ModSetting.FriendsAlways].BoxedValue;

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
                ViewManager.Instance.gameMenuView.View.ExecuteScript(ResourcesHandler.GetEmbeddedResource("mods_extension.js"));
                ViewManager.Instance.gameMenuView.View.ExecuteScript(ResourcesHandler.GetEmbeddedResource("mod_menu.js"));
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

        static void OnToggleUpdate(string p_name, string p_value)
        {
            try
            {
                if(Enum.TryParse(p_name, out ModSetting l_setting) && bool.TryParse(p_value, out bool l_value))
                {
                    switch(l_setting)
                    {
                        case ModSetting.Delay:
                        {
                            Delay = l_value;
                            OnDelayChange.Invoke(Delay);
                        }
                        break;

                        case ModSetting.NotifyInPublic:
                        {
                            NotifyInPublic = l_value;
                            OnNotifyInPublicChanged.Invoke(NotifyInPublic);
                        }
                        break;

                        case ModSetting.NotifyInFriends:
                        {
                            NotifyInFriends = l_value;
                            OnNotifyInFriendsChanged.Invoke(NotifyInFriends);
                        }
                        break;

                        case ModSetting.NotifyInPrivate:
                        {
                            NotifyInPrivate = l_value;
                            OnNotifyInPrivateChanged.Invoke(NotifyInPrivate);
                        }
                        break;

                        case ModSetting.FriendsAlways:
                        {
                            FriendsAlways = l_value;
                            OnFriendsAlwaysChanged.Invoke(FriendsAlways);
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
                        case ModSetting.Volume:
                        {
                            Volume = l_value * 0.01f;
                            OnVolumeChanged.Invoke(Volume);
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
                        case ModSetting.NotifyType:
                        {
                            NotifyType = (NotificationType)l_value;
                            OnNotifyTypeChanged.Invoke(NotifyType);
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

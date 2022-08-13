using ABI_RC.Core.InteractionSystem;
using cohtml;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ml_lme
{
    static class Settings
    {
        public enum LeapTrackingMode
        {
            Screentop = 0,
            Desktop,
            HMD
        }

        enum ModSetting
        {
            Enabled,
            DesktopX,
            DesktopY,
            DesktopZ,
            FingersOnly,
            Model,
            Mode,
            Angle,
            Head,
            HeadX,
            HeadY,
            HeadZ
        };

        static bool ms_enabled = false;
        static Vector3 ms_desktopOffset = new Vector3(0f, -0.45f, 0.3f);
        static bool ms_fingersOnly = false;
        static bool ms_modelVisibility = false;
        static LeapTrackingMode ms_trackingMode = LeapTrackingMode.Desktop;
        static float ms_rootAngle = 0f;
        static bool ms_headAttach = false;
        static Vector3 ms_headOffset = new Vector3(0f, -0.3f, 0.15f);

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        static public event Action<bool> EnabledChange;
        static public event Action<Vector3> DesktopOffsetChange;
        static public event Action<bool> FingersOnlyChange;
        static public event Action<bool> ModelVisibilityChange;
        static public event Action<LeapTrackingMode> TrackingModeChange;
        static public event Action<float> RootAngleChange;
        static public event Action<bool> HeadAttachChange;
        static public event Action<Vector3> HeadOffsetChange;

        public static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("LME");

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>();
            ms_entries.Add(ms_category.CreateEntry(ModSetting.Enabled.ToString(), ms_enabled));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.DesktopX.ToString(), 0));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.DesktopY.ToString(), -45));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.DesktopZ.ToString(), 30));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.FingersOnly.ToString(), ms_modelVisibility));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.Model.ToString(), ms_modelVisibility));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.Mode.ToString(), (int)ms_trackingMode));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.Angle.ToString(), 0));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.Head.ToString(), ms_headAttach));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.HeadX.ToString(), 0));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.HeadY.ToString(), -30));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.HeadZ.ToString(), 15));

            Load();

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
                ViewManager.Instance.gameMenuView.View.BindCall("MelonMod_LME_Call_InpToggle", new Action<string, string>(OnToggleUpdate));
                ViewManager.Instance.gameMenuView.View.BindCall("MelonMod_LME_Call_InpSlider", new Action<string, string>(OnSliderUpdate));
                ViewManager.Instance.gameMenuView.View.BindCall("MelonMod_LME_Call_InpDropdown", new Action<string, string>(OnDropdownUpdate));
            };
            ViewManager.Instance.gameMenuView.Listener.FinishLoad += (_) =>
            {
                ViewManager.Instance.gameMenuView.View.ExecuteScript(Scripts.GetEmbeddedScript("menu.js"));
                foreach(var l_entry in ms_entries)
                    ViewManager.Instance.gameMenuView.View.TriggerEvent("updateModSettingLME", l_entry.DisplayName, l_entry.GetValueAsString());
            };
        }

        static void Load()
        {
            ms_enabled = (bool)ms_entries[(int)ModSetting.Enabled].BoxedValue;
            ms_desktopOffset = new Vector3(
                (int)ms_entries[(int)ModSetting.DesktopX].BoxedValue,
                (int)ms_entries[(int)ModSetting.DesktopY].BoxedValue,
                (int)ms_entries[(int)ModSetting.DesktopZ].BoxedValue
            ) * 0.01f;
            ms_fingersOnly = (bool)ms_entries[(int)ModSetting.FingersOnly].BoxedValue;
            ms_modelVisibility = (bool)ms_entries[(int)ModSetting.Model].BoxedValue;
            ms_trackingMode = (LeapTrackingMode)(int)ms_entries[(int)ModSetting.Mode].BoxedValue;
            ms_rootAngle = (int)ms_entries[(int)ModSetting.Angle].BoxedValue;
            ms_headAttach = (bool)ms_entries[(int)ModSetting.Head].BoxedValue;
            ms_headOffset = new Vector3(
                (int)ms_entries[(int)ModSetting.HeadX].BoxedValue,
                (int)ms_entries[(int)ModSetting.HeadY].BoxedValue,
                (int)ms_entries[(int)ModSetting.HeadZ].BoxedValue
            ) * 0.01f;
        }

        static void OnToggleUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.Enabled:
                    {
                        ms_enabled = bool.Parse(p_value);
                        EnabledChange?.Invoke(ms_enabled);
                    }
                    break;

                    case ModSetting.FingersOnly:
                    {
                        ms_fingersOnly = bool.Parse(p_value);
                        FingersOnlyChange?.Invoke(ms_fingersOnly);
                    }
                    break;

                    case ModSetting.Model:
                    {
                        ms_modelVisibility = bool.Parse(p_value);
                        ModelVisibilityChange?.Invoke(ms_modelVisibility);
                    }
                    break;

                    case ModSetting.Head:
                    {
                        ms_headAttach = bool.Parse(p_value);
                        HeadAttachChange?.Invoke(ms_headAttach);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = bool.Parse(p_value);
            }
        }

        static void OnSliderUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.DesktopX:
                    {
                        ms_desktopOffset.x = int.Parse(p_value) * 0.01f;
                        DesktopOffsetChange?.Invoke(ms_desktopOffset);
                    }
                    break;
                    case ModSetting.DesktopY:
                    {
                        ms_desktopOffset.y = int.Parse(p_value) * 0.01f;
                        DesktopOffsetChange?.Invoke(ms_desktopOffset);
                    }
                    break;
                    case ModSetting.DesktopZ:
                    {
                        ms_desktopOffset.z = int.Parse(p_value) * 0.01f;
                        DesktopOffsetChange?.Invoke(ms_desktopOffset);
                    }
                    break;

                    case ModSetting.Angle:
                    {
                        ms_rootAngle = int.Parse(p_value);
                        RootAngleChange?.Invoke(ms_rootAngle);
                    }
                    break;

                    case ModSetting.HeadX:
                    {
                        ms_headOffset.x = int.Parse(p_value) * 0.01f;
                        HeadOffsetChange?.Invoke(ms_headOffset);
                    }
                    break;
                    case ModSetting.HeadY:
                    {
                        ms_headOffset.y = int.Parse(p_value) * 0.01f;
                        HeadOffsetChange?.Invoke(ms_headOffset);
                    }
                    break;
                    case ModSetting.HeadZ:
                    {
                        ms_headOffset.z = int.Parse(p_value) * 0.01f;
                        HeadOffsetChange?.Invoke(ms_headOffset);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = int.Parse(p_value);
            }
        }

        static void OnDropdownUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.Mode:
                    {
                        ms_trackingMode = (LeapTrackingMode)int.Parse(p_value);
                        TrackingModeChange?.Invoke(ms_trackingMode);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = int.Parse(p_value);
            }
        }

        public static bool Enabled
        {
            get => ms_enabled;
        }

        public static Vector3 DesktopOffset
        {
            get => ms_desktopOffset;
        }

        public static bool FingersOnly
        {
            get => ms_fingersOnly;
        }

        public static bool ModelVisibility
        {
            get => ms_modelVisibility;
        }

        public static LeapTrackingMode TrackingMode
        {
            get => ms_trackingMode;
        }

        public static float RootAngle
        {
            get => ms_rootAngle;
        }

        public static bool HeadAttach
        {
            get => ms_headAttach;
        }

        public static Vector3 HeadOffset
        {
            get => ms_headOffset;
        }
    }
}

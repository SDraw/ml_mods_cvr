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
            AngleX,
            AngleY,
            AngleZ,
            Head,
            HeadX,
            HeadY,
            HeadZ,
            TrackElbows,
            Input,
            InteractThreadhold,
            GripThreadhold,
            VisualHands
        };

        public static bool Enabled { get; private set; } = false;
        public static Vector3 DesktopOffset { get; private set; } = new Vector3(0f, -0.45f, 0.3f);
        public static bool FingersOnly { get; private set; } = false;
        public static bool ModelVisibility { get; private set; } = false;
        public static LeapTrackingMode TrackingMode { get; private set; } = LeapTrackingMode.Desktop;
        public static Vector3 RootAngle { get; private set; } = Vector3.zero;
        public static bool HeadAttach { get; private set; } = false;
        public static Vector3 HeadOffset { get; private set; } = new Vector3(0f, -0.3f, 0.15f);
        public static bool TrackElbows { get; private set; } = true;
        public static bool Input { get; private set; } = true;
        public static float InteractThreadhold { get; private set; } = 0.8f;
        public static float GripThreadhold { get; private set; } = 0.4f;
        public static bool VisualHands { get; private set; } = false;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        static public event Action<bool> EnabledChange;
        static public event Action<Vector3> DesktopOffsetChange;
        static public event Action<bool> FingersOnlyChange;
        static public event Action<bool> ModelVisibilityChange;
        static public event Action<LeapTrackingMode> TrackingModeChange;
        static public event Action<Vector3> RootAngleChange;
        static public event Action<bool> HeadAttachChange;
        static public event Action<Vector3> HeadOffsetChange;
        static public event Action<bool> TrackElbowsChange;
        static public event Action<bool> InputChange;
        static public event Action<float> InteractThreadholdChange;
        static public event Action<float> GripThreadholdChange;
        static public event Action<bool> VisualHandsChange;

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("LME", null, true);

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.Enabled.ToString(), Enabled),
                ms_category.CreateEntry(ModSetting.DesktopX.ToString(), (int)(DesktopOffset.x * 100f)),
                ms_category.CreateEntry(ModSetting.DesktopY.ToString(), (int)(DesktopOffset.y * 100f)),
                ms_category.CreateEntry(ModSetting.DesktopZ.ToString(), (int)(DesktopOffset.z * 100f)),
                ms_category.CreateEntry(ModSetting.FingersOnly.ToString(), FingersOnly),
                ms_category.CreateEntry(ModSetting.Model.ToString(), ModelVisibility),
                ms_category.CreateEntry(ModSetting.Mode.ToString(), (int)TrackingMode),
                ms_category.CreateEntry(ModSetting.AngleX.ToString(), (int)(RootAngle.x * 100f)),
                ms_category.CreateEntry(ModSetting.AngleY.ToString(), (int)(RootAngle.y * 100f)),
                ms_category.CreateEntry(ModSetting.AngleZ.ToString(), (int)(RootAngle.z * 100f)),
                ms_category.CreateEntry(ModSetting.Head.ToString(), HeadAttach),
                ms_category.CreateEntry(ModSetting.HeadX.ToString(), (int)(HeadOffset.x * 100f)),
                ms_category.CreateEntry(ModSetting.HeadY.ToString(), (int)(HeadOffset.y * 100f)),
                ms_category.CreateEntry(ModSetting.HeadZ.ToString(), (int)(HeadOffset.z * 100f)),
                ms_category.CreateEntry(ModSetting.TrackElbows.ToString(), TrackElbows),
                ms_category.CreateEntry(ModSetting.Input.ToString(), Input),
                ms_category.CreateEntry(ModSetting.InteractThreadhold.ToString(), (int)(InteractThreadhold * 100f)),
                ms_category.CreateEntry(ModSetting.GripThreadhold.ToString(), (int)(GripThreadhold * 100f)),
                ms_category.CreateEntry(ModSetting.VisualHands.ToString(), VisualHands)
            };

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
            Enabled = (bool)ms_entries[(int)ModSetting.Enabled].BoxedValue;
            DesktopOffset = new Vector3(
                (int)ms_entries[(int)ModSetting.DesktopX].BoxedValue,
                (int)ms_entries[(int)ModSetting.DesktopY].BoxedValue,
                (int)ms_entries[(int)ModSetting.DesktopZ].BoxedValue
            ) * 0.01f;
            FingersOnly = (bool)ms_entries[(int)ModSetting.FingersOnly].BoxedValue;
            ModelVisibility = (bool)ms_entries[(int)ModSetting.Model].BoxedValue;
            TrackingMode = (LeapTrackingMode)(int)ms_entries[(int)ModSetting.Mode].BoxedValue;
            RootAngle = new Vector3(
                (int)ms_entries[(int)ModSetting.AngleX].BoxedValue,
                (int)ms_entries[(int)ModSetting.AngleY].BoxedValue,
                (int)ms_entries[(int)ModSetting.AngleZ].BoxedValue
            );
            HeadAttach = (bool)ms_entries[(int)ModSetting.Head].BoxedValue;
            HeadOffset = new Vector3(
                (int)ms_entries[(int)ModSetting.HeadX].BoxedValue,
                (int)ms_entries[(int)ModSetting.HeadY].BoxedValue,
                (int)ms_entries[(int)ModSetting.HeadZ].BoxedValue
            ) * 0.01f;
            TrackElbows = (bool)ms_entries[(int)ModSetting.TrackElbows].BoxedValue;
            Input = (bool)ms_entries[(int)ModSetting.Input].BoxedValue;
            InteractThreadhold = (int)ms_entries[(int)ModSetting.InteractThreadhold].BoxedValue * 0.01f;
            GripThreadhold = (int)ms_entries[(int)ModSetting.GripThreadhold].BoxedValue * 0.01f;
            VisualHands = (bool)ms_entries[(int)ModSetting.VisualHands].BoxedValue;
        }

        static void OnToggleUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.Enabled:
                    {
                        Enabled = bool.Parse(p_value);
                        EnabledChange?.Invoke(Enabled);
                    }
                    break;

                    case ModSetting.FingersOnly:
                    {
                        FingersOnly = bool.Parse(p_value);
                        FingersOnlyChange?.Invoke(FingersOnly);
                    }
                    break;

                    case ModSetting.Model:
                    {
                        ModelVisibility = bool.Parse(p_value);
                        ModelVisibilityChange?.Invoke(ModelVisibility);
                    }
                    break;

                    case ModSetting.Head:
                    {
                        HeadAttach = bool.Parse(p_value);
                        HeadAttachChange?.Invoke(HeadAttach);
                    }
                    break;

                    case ModSetting.TrackElbows:
                    {
                        TrackElbows = bool.Parse(p_value);
                        TrackElbowsChange?.Invoke(TrackElbows);
                    }
                    break;

                    case ModSetting.Input:
                    {
                        Input = bool.Parse(p_value);
                        InputChange?.Invoke(Input);
                    }
                    break;

                    case ModSetting.VisualHands:
                    {
                        VisualHands = bool.Parse(p_value);
                        VisualHandsChange?.Invoke(VisualHands);
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
                        Vector3 l_current = DesktopOffset;
                        l_current.x = int.Parse(p_value) * 0.01f;
                        DesktopOffset = l_current;
                        DesktopOffsetChange?.Invoke(l_current);
                    }
                    break;
                    case ModSetting.DesktopY:
                    {
                        Vector3 l_current = DesktopOffset;
                        l_current.y = int.Parse(p_value) * 0.01f;
                        DesktopOffset = l_current;
                        DesktopOffsetChange?.Invoke(l_current);
                    }
                    break;
                    case ModSetting.DesktopZ:
                    {
                        Vector3 l_current = DesktopOffset;
                        l_current.z = int.Parse(p_value) * 0.01f;
                        DesktopOffset = l_current;
                        DesktopOffsetChange?.Invoke(l_current);
                    }
                    break;

                    case ModSetting.AngleX:
                    {
                        Vector3 l_current = RootAngle;
                        l_current.x = int.Parse(p_value);
                        RootAngle = l_current;
                        RootAngleChange?.Invoke(l_current);
                    }
                    break;

                    case ModSetting.AngleY:
                    {
                        Vector3 l_current = RootAngle;
                        l_current.y = int.Parse(p_value);
                        RootAngle = l_current;
                        RootAngleChange?.Invoke(l_current);
                    }
                    break;

                    case ModSetting.AngleZ:
                    {
                        Vector3 l_current = RootAngle;
                        l_current.z = int.Parse(p_value);
                        RootAngle = l_current;
                        RootAngleChange?.Invoke(l_current);
                    }
                    break;

                    case ModSetting.HeadX:
                    {
                        Vector3 l_current = HeadOffset;
                        l_current.x = int.Parse(p_value) * 0.01f;
                        HeadOffset = l_current;
                        HeadOffsetChange?.Invoke(l_current);
                    }
                    break;
                    case ModSetting.HeadY:
                    {
                        Vector3 l_current = HeadOffset;
                        l_current.y = int.Parse(p_value) * 0.01f;
                        HeadOffset = l_current;
                        HeadOffsetChange?.Invoke(l_current);
                    }
                    break;
                    case ModSetting.HeadZ:
                    {
                        Vector3 l_current = HeadOffset;
                        l_current.z = int.Parse(p_value) * 0.01f;
                        HeadOffset = l_current;
                        HeadOffsetChange?.Invoke(l_current);
                    }
                    break;
                    case ModSetting.InteractThreadhold:
                    {
                        InteractThreadhold = int.Parse(p_value) * 0.01f;
                        InteractThreadholdChange?.Invoke(InteractThreadhold);
                    }
                    break;
                    case ModSetting.GripThreadhold:
                    {
                        GripThreadhold = int.Parse(p_value) * 0.01f;
                        GripThreadholdChange?.Invoke(GripThreadhold);
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
                        TrackingMode = (LeapTrackingMode)int.Parse(p_value);
                        TrackingModeChange?.Invoke(TrackingMode);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = int.Parse(p_value);
            }
        }
    }
}

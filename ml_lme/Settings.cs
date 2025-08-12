using ABI_RC.Core.InteractionSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ml_lme
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
            Interaction,
            Gestures,
            InteractThreadhold,
            GripThreadhold,
            VisualHands,
            MechanimFilter
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
        public static bool Interaction { get; private set; } = true;
        public static bool Gestures { get; private set; } = false;
        public static float InteractThreadhold { get; private set; } = 0.8f;
        public static float GripThreadhold { get; private set; } = 0.4f;
        public static bool VisualHands { get; private set; } = false;
        public static bool MechanimFilter { get; private set; } = true;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        public static readonly SettingEvent<bool> OnEnabledChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<Vector3> OnDesktopOffsetChanged = new SettingEvent<Vector3>();
        public static readonly SettingEvent<bool> OnFingersOnlyChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnModelVisibilityChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<LeapTrackingMode> OnTrackingModeChanged = new SettingEvent<LeapTrackingMode>();
        public static readonly SettingEvent<Vector3> OnRootAngleChanged = new SettingEvent<Vector3>();
        public static readonly SettingEvent<bool> OnHeadAttachChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<Vector3> OnHeadOffsetChanged = new SettingEvent<Vector3>();
        public static readonly SettingEvent<bool> OnTrackElbowsChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnInteractionChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnGesturesChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<float> OnInteractThreadholdChanged = new SettingEvent<float>();
        public static readonly SettingEvent<float> OnGripThreadholdChanged = new SettingEvent<float>();
        public static readonly SettingEvent<bool> OnVisualHandsChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnMechanimFilterChanged = new SettingEvent<bool>();

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
                ms_category.CreateEntry(ModSetting.Interaction.ToString(), Interaction),
                ms_category.CreateEntry(ModSetting.Gestures.ToString(), Gestures),
                ms_category.CreateEntry(ModSetting.InteractThreadhold.ToString(), (int)(InteractThreadhold * 100f)),
                ms_category.CreateEntry(ModSetting.GripThreadhold.ToString(), (int)(GripThreadhold * 100f)),
                ms_category.CreateEntry(ModSetting.VisualHands.ToString(), VisualHands),
                ms_category.CreateEntry(ModSetting.MechanimFilter.ToString(), MechanimFilter)
            };

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
            Interaction = (bool)ms_entries[(int)ModSetting.Interaction].BoxedValue;
            Gestures = (bool)ms_entries[(int)ModSetting.Gestures].BoxedValue;
            InteractThreadhold = (int)ms_entries[(int)ModSetting.InteractThreadhold].BoxedValue * 0.01f;
            GripThreadhold = (int)ms_entries[(int)ModSetting.GripThreadhold].BoxedValue * 0.01f;
            VisualHands = (bool)ms_entries[(int)ModSetting.VisualHands].BoxedValue;
            MechanimFilter = (bool)ms_entries[(int)ModSetting.MechanimFilter].BoxedValue;

            MelonLoader.MelonCoroutines.Start(WaitMainMenuUi());
        }

        static System.Collections.IEnumerator WaitMainMenuUi()
        {
            while(ViewManager.Instance == null)
                yield return null;
            while(ViewManager.Instance.cohtmlView == null)
                yield return null;
            while(ViewManager.Instance.cohtmlView.Listener == null)
                yield return null;

            ViewManager.Instance.cohtmlView.Listener.ReadyForBindings += () =>
            {
                ViewManager.Instance.cohtmlView.View.BindCall("OnToggleUpdate_" + ms_category.Identifier, new Action<string, string>(OnToggleUpdate));
                ViewManager.Instance.cohtmlView.View.BindCall("OnSliderUpdate_" + ms_category.Identifier, new Action<string, string>(OnSliderUpdate));
                ViewManager.Instance.cohtmlView.View.BindCall("OnDropdownUpdate_" + ms_category.Identifier, new Action<string, string>(OnDropdownUpdate));
            };
            ViewManager.Instance.cohtmlView.Listener.FinishLoad += (_) =>
            {
                ViewManager.Instance.cohtmlView.View.ExecuteScript(ResourcesHandler.GetEmbeddedResource("mods_extension.js"));
                ViewManager.Instance.cohtmlView.View.ExecuteScript(ResourcesHandler.GetEmbeddedResource("mod_menu.js"));
                MelonLoader.MelonCoroutines.Start(UpdateMenuSettings());
            };
        }

        static System.Collections.IEnumerator UpdateMenuSettings()
        {
            while(!ViewManager.Instance.IsReady || !ViewManager.Instance.IsViewShown)
                yield return null;

            foreach(var l_entry in ms_entries)
                ViewManager.Instance.cohtmlView.View.TriggerEvent("updateModSetting", ms_category.Identifier, l_entry.DisplayName, l_entry.GetValueAsString());
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

                        case ModSetting.FingersOnly:
                        {
                            FingersOnly = l_value;
                            OnFingersOnlyChanged.Invoke(FingersOnly);
                        }
                        break;

                        case ModSetting.Model:
                        {
                            ModelVisibility = l_value;
                            OnModelVisibilityChanged.Invoke(ModelVisibility);
                        }
                        break;

                        case ModSetting.Head:
                        {
                            HeadAttach = l_value;
                            OnHeadAttachChanged.Invoke(HeadAttach);
                        }
                        break;

                        case ModSetting.TrackElbows:
                        {
                            TrackElbows = l_value;
                            OnTrackElbowsChanged.Invoke(TrackElbows);
                        }
                        break;

                        case ModSetting.Interaction:
                        {
                            Interaction = l_value;
                            OnInteractionChanged.Invoke(Interaction);
                        }
                        break;

                        case ModSetting.Gestures:
                        {
                            Gestures = l_value;
                            OnGesturesChanged.Invoke(Gestures);
                        }
                        break;

                        case ModSetting.VisualHands:
                        {
                            VisualHands = l_value;
                            OnVisualHandsChanged.Invoke(VisualHands);
                        }
                        break;

                        case ModSetting.MechanimFilter:
                        {
                            MechanimFilter = l_value;
                            OnMechanimFilterChanged.Invoke(MechanimFilter);
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
                        case ModSetting.DesktopX:
                        {
                            Vector3 l_current = DesktopOffset;
                            l_current.x = l_value * 0.01f;
                            DesktopOffset = l_current;
                            OnDesktopOffsetChanged.Invoke(l_current);
                        }
                        break;
                        case ModSetting.DesktopY:
                        {
                            Vector3 l_current = DesktopOffset;
                            l_current.y = l_value * 0.01f;
                            DesktopOffset = l_current;
                            OnDesktopOffsetChanged.Invoke(l_current);
                        }
                        break;
                        case ModSetting.DesktopZ:
                        {
                            Vector3 l_current = DesktopOffset;
                            l_current.z = l_value * 0.01f;
                            DesktopOffset = l_current;
                            OnDesktopOffsetChanged.Invoke(l_current);
                        }
                        break;

                        case ModSetting.AngleX:
                        {
                            Vector3 l_current = RootAngle;
                            l_current.x = l_value;
                            RootAngle = l_current;
                            OnRootAngleChanged.Invoke(l_current);
                        }
                        break;

                        case ModSetting.AngleY:
                        {
                            Vector3 l_current = RootAngle;
                            l_current.y = l_value;
                            RootAngle = l_current;
                            OnRootAngleChanged.Invoke(l_current);
                        }
                        break;

                        case ModSetting.AngleZ:
                        {
                            Vector3 l_current = RootAngle;
                            l_current.z = l_value;
                            RootAngle = l_current;
                            OnRootAngleChanged.Invoke(l_current);
                        }
                        break;

                        case ModSetting.HeadX:
                        {
                            Vector3 l_current = HeadOffset;
                            l_current.x = l_value * 0.01f;
                            HeadOffset = l_current;
                            OnHeadOffsetChanged.Invoke(l_current);
                        }
                        break;
                        case ModSetting.HeadY:
                        {
                            Vector3 l_current = HeadOffset;
                            l_current.y = l_value * 0.01f;
                            HeadOffset = l_current;
                            OnHeadOffsetChanged.Invoke(l_current);
                        }
                        break;
                        case ModSetting.HeadZ:
                        {
                            Vector3 l_current = HeadOffset;
                            l_current.z = l_value * 0.01f;
                            HeadOffset = l_current;
                            OnHeadOffsetChanged.Invoke(l_current);
                        }
                        break;
                        case ModSetting.InteractThreadhold:
                        {
                            InteractThreadhold = l_value * 0.01f;
                            OnInteractThreadholdChanged.Invoke(InteractThreadhold);
                        }
                        break;
                        case ModSetting.GripThreadhold:
                        {
                            GripThreadhold = l_value * 0.01f;
                            OnGripThreadholdChanged.Invoke(GripThreadhold);
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
                        case ModSetting.Mode:
                        {
                            TrackingMode = (LeapTrackingMode)l_value;
                            OnTrackingModeChanged.Invoke(TrackingMode);
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

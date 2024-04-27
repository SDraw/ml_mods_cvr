using System;
using System.Collections.Generic;

namespace ml_pmc
{
    static class Settings
    {
        internal class SettingEvent<T>
        {
            event Action<T> m_action;
            public void AddHandler(Action<T> p_listener) => m_action += p_listener;
            public void RemoveHandler(Action<T> p_listener) => m_action -= p_listener;
            public void Invoke(T p_value) => m_action?.Invoke(p_value);
        }

        public enum ModSetting
        {
            Position,
            Rotation,
            Gestures,
            LookAtMix,
            MirrorPose,
            MirrorPosition,
            MirrorRotation
        }

        public static bool Position { get; private set; } = true;
        public static bool Rotation { get; private set; } = true;
        public static bool Gestures { get; private set; } = true;
        public static bool LookAtMix { get; private set; } = true;
        public static bool MirrorPose { get; private set; } = false;
        public static bool MirrorPosition { get; private set; } = false;
        public static bool MirrorRotation { get; private set; } = false;

        public static readonly SettingEvent<bool> OnPositionChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnRotationChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnGesturesChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnLookAtMixChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnMirrorPoseChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnMirrorPositionChanged = new SettingEvent<bool>();
        public static readonly SettingEvent<bool> OnMirrorRotationChanged = new SettingEvent<bool>();

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("PMC", null, true);
            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.Position.ToString(), Position),
                ms_category.CreateEntry(ModSetting.Rotation.ToString(), Rotation),
                ms_category.CreateEntry(ModSetting.Gestures.ToString(), Gestures),
                ms_category.CreateEntry(ModSetting.LookAtMix.ToString(), LookAtMix),
                ms_category.CreateEntry(ModSetting.MirrorPose.ToString(), MirrorPose),
                ms_category.CreateEntry(ModSetting.MirrorPosition.ToString(), MirrorPosition),
                ms_category.CreateEntry(ModSetting.MirrorRotation.ToString(), MirrorRotation),
            };

            Position = (bool)ms_entries[(int)ModSetting.Position].BoxedValue;
            Rotation = (bool)ms_entries[(int)ModSetting.Rotation].BoxedValue;
            Gestures = (bool)ms_entries[(int)ModSetting.Gestures].BoxedValue;
            LookAtMix = (bool)ms_entries[(int)ModSetting.LookAtMix].BoxedValue;
            MirrorPose = (bool)ms_entries[(int)ModSetting.MirrorPose].BoxedValue;
            MirrorPosition = (bool)ms_entries[(int)ModSetting.MirrorPosition].BoxedValue;
            MirrorRotation = (bool)ms_entries[(int)ModSetting.MirrorRotation].BoxedValue;
        }

        public static void SetSetting(ModSetting p_setting, object p_value)
        {
            try
            {
                switch(p_setting)
                {
                    case ModSetting.Position:
                    {
                        Position = (bool)p_value;
                        OnPositionChanged.Invoke(Position);
                    }
                    break;

                    case ModSetting.Rotation:
                    {
                        Rotation = (bool)p_value;
                        OnRotationChanged.Invoke(Rotation);
                        break;
                    }

                    case ModSetting.Gestures:
                    {
                        Gestures = (bool)p_value;
                        OnGesturesChanged.Invoke(Gestures);
                    }
                    break;

                    case ModSetting.LookAtMix:
                    {
                        LookAtMix = (bool)p_value;
                        OnLookAtMixChanged.Invoke(LookAtMix);
                    }
                    break;

                    case ModSetting.MirrorPose:
                    {
                        MirrorPose = (bool)p_value;
                        OnMirrorPoseChanged.Invoke(MirrorPose);
                    }
                    break;

                    case ModSetting.MirrorPosition:
                    {
                        MirrorPosition = (bool)p_value;
                        OnMirrorPositionChanged.Invoke(MirrorPosition);
                    }
                    break;

                    case ModSetting.MirrorRotation:
                    {
                        MirrorRotation = (bool)p_value;
                        OnMirrorRotationChanged.Invoke(MirrorRotation);
                    }
                    break;
                }

                if(ms_entries != null)
                    ms_entries[(int)p_setting].BoxedValue = p_value;
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

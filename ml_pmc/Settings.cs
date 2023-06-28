using System;
using System.Collections.Generic;

namespace ml_pmc
{
    static class Settings
    {
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

        public static Action<bool> PositionChange;
        public static Action<bool> RotationChange;
        public static Action<bool> GesturesChange;
        public static Action<bool> LookAtMixChange;
        public static Action<bool> MirrorPoseChange;
        public static Action<bool> MirrorPositionChange;
        public static Action<bool> MirrorRotationChange;

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
            switch(p_setting)
            {
                case ModSetting.Position:
                {
                    Position = (bool)p_value;
                    PositionChange?.Invoke((bool)p_value);
                }
                break;

                case ModSetting.Rotation:
                {
                    Rotation = (bool)p_value;
                    RotationChange?.Invoke((bool)p_value);
                    break;
                }

                case ModSetting.Gestures:
                {
                    Gestures = (bool)p_value;
                    GesturesChange?.Invoke((bool)p_value);
                }
                break;

                case ModSetting.LookAtMix:
                {
                    LookAtMix = (bool)p_value;
                    LookAtMixChange?.Invoke((bool)p_value);
                }
                break;

                //
                case ModSetting.MirrorPose:
                {
                    MirrorPose = (bool)p_value;
                    MirrorPoseChange?.Invoke((bool)p_value);
                }
                break;

                case ModSetting.MirrorPosition:
                {
                    MirrorPosition = (bool)p_value;
                    MirrorPositionChange?.Invoke((bool)p_value);
                }
                break;

                case ModSetting.MirrorRotation:
                {
                    MirrorRotation = (bool)p_value;
                    MirrorRotationChange?.Invoke((bool)p_value);
                }
                break;
            }

            if(ms_entries != null)
                ms_entries[(int)p_setting].BoxedValue = p_value;
        }
    }
}

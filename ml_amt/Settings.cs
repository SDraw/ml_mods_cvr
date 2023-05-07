using ABI_RC.Core.InteractionSystem;
using cohtml;
using System;
using System.Collections.Generic;

namespace ml_amt
{
    static class Settings
    {
        enum ModSetting
        {
            IKOverrideCrouch = 0,
            CrouchLimit,
            IKOverrideProne,
            ProneLimit,
            PoseTransitions,
            AdjustedMovement,
            IKOverrideFly,
            IKOverrideJump,
            DetectEmotes,
            FollowHips,
            CollisionScale,
            ScaledSteps,
            ScaledJump,
            MassCenter,
            OverrideFix
        };

        public static bool IKOverrideCrouch { get; private set; } = true;
        public static float CrouchLimit { get; private set; } = 0.65f;
        public static bool IKOverrideProne { get; private set; } = true;
        public static float ProneLimit { get; private set; } = 0.3f;
        public static bool PoseTransitions { get; private set; } = true;
        public static bool AdjustedMovement { get; private set; } = true;
        public static bool IKOverrideFly { get; private set; } = true;
        public static bool IKOverrideJump { get; private set; } = true;
        public static bool DetectEmotes { get; private set; } = true;
        public static bool FollowHips { get; private set; } = true;
        public static bool MassCenter { get; private set; } = true;
        public static bool ScaledSteps { get; private set; } = true;
        public static bool ScaledJump { get; private set; } = false;
        public static bool CollisionScale { get; private set; } = true;
        public static bool OverrideFix { get; private set; } = true;

        static MelonLoader.MelonPreferences_Category ms_category = null;
        static List<MelonLoader.MelonPreferences_Entry> ms_entries = null;

        static public event Action<bool> IKOverrideCrouchChange;
        static public event Action<float> CrouchLimitChange;
        static public event Action<bool> IKOverrideProneChange;
        static public event Action<float> ProneLimitChange;
        static public event Action<bool> PoseTransitionsChange;
        static public event Action<bool> AdjustedMovementChange;
        static public event Action<bool> IKOverrideFlyChange;
        static public event Action<bool> IKOverrideJumpChange;
        static public event Action<bool> DetectEmotesChange;
        static public event Action<bool> FollowHipsChange;
        static public event Action<bool> MassCenterChange;
        static public event Action<bool> ScaledStepsChange;
        static public event Action<bool> ScaledJumpChange;
        static public event Action<bool> CollisionScaleChange;
        static public event Action<bool> OverrideFixChange;

        internal static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("AMT", null, true);

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>()
            {
                ms_category.CreateEntry(ModSetting.IKOverrideCrouch.ToString(), IKOverrideCrouch),
                ms_category.CreateEntry(ModSetting.CrouchLimit.ToString(), (int)(CrouchLimit * 100f)),
                ms_category.CreateEntry(ModSetting.IKOverrideProne.ToString(), IKOverrideProne),
                ms_category.CreateEntry(ModSetting.ProneLimit.ToString(), (int)(ProneLimit * 100f)),
                ms_category.CreateEntry(ModSetting.PoseTransitions.ToString(), PoseTransitions),
                ms_category.CreateEntry(ModSetting.AdjustedMovement.ToString(), AdjustedMovement),
                ms_category.CreateEntry(ModSetting.IKOverrideFly.ToString(), IKOverrideFly),
                ms_category.CreateEntry(ModSetting.IKOverrideJump.ToString(), IKOverrideJump),
                ms_category.CreateEntry(ModSetting.DetectEmotes.ToString(), DetectEmotes),
                ms_category.CreateEntry(ModSetting.FollowHips.ToString(), FollowHips),
                ms_category.CreateEntry(ModSetting.MassCenter.ToString(), MassCenter),
                ms_category.CreateEntry(ModSetting.ScaledSteps.ToString(), ScaledSteps),
                ms_category.CreateEntry(ModSetting.ScaledJump.ToString(), ScaledJump),
                ms_category.CreateEntry(ModSetting.CollisionScale.ToString(), CollisionScale),
                ms_category.CreateEntry(ModSetting.OverrideFix.ToString(), OverrideFix)
            };

            IKOverrideCrouch = (bool)ms_entries[(int)ModSetting.IKOverrideCrouch].BoxedValue;
            CrouchLimit = ((int)ms_entries[(int)ModSetting.CrouchLimit].BoxedValue) * 0.01f;
            IKOverrideProne = (bool)ms_entries[(int)ModSetting.IKOverrideProne].BoxedValue;
            ProneLimit = ((int)ms_entries[(int)ModSetting.ProneLimit].BoxedValue) * 0.01f;
            PoseTransitions = (bool)ms_entries[(int)ModSetting.PoseTransitions].BoxedValue;
            AdjustedMovement = (bool)ms_entries[(int)ModSetting.AdjustedMovement].BoxedValue;
            IKOverrideFly = (bool)ms_entries[(int)ModSetting.IKOverrideFly].BoxedValue;
            IKOverrideJump = (bool)ms_entries[(int)ModSetting.IKOverrideJump].BoxedValue;
            DetectEmotes = (bool)ms_entries[(int)ModSetting.DetectEmotes].BoxedValue;
            FollowHips = (bool)ms_entries[(int)ModSetting.FollowHips].BoxedValue;
            MassCenter = (bool)ms_entries[(int)ModSetting.MassCenter].BoxedValue;
            ScaledSteps = (bool)ms_entries[(int)ModSetting.ScaledSteps].BoxedValue;
            ScaledJump = (bool)ms_entries[(int)ModSetting.ScaledJump].BoxedValue;
            CollisionScale = (bool)ms_entries[(int)ModSetting.CollisionScale].BoxedValue;
            OverrideFix = (bool)ms_entries[(int)ModSetting.OverrideFix].BoxedValue;

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
                ViewManager.Instance.gameMenuView.View.BindCall("MelonMod_AMT_Call_InpSlider", new Action<string, string>(OnSliderUpdate));
                ViewManager.Instance.gameMenuView.View.BindCall("MelonMod_AMT_Call_InpToggle", new Action<string, string>(OnToggleUpdate));
            };
            ViewManager.Instance.gameMenuView.Listener.FinishLoad += (_) =>
            {
                ViewManager.Instance.gameMenuView.View.ExecuteScript(Scripts.GetEmbeddedScript("menu.js"));
                foreach(var l_entry in ms_entries)
                    ViewManager.Instance.gameMenuView.View.TriggerEvent("updateModSettingAMT", l_entry.DisplayName, l_entry.GetValueAsString());
            };
        }

        static void OnSliderUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.CrouchLimit:
                    {
                        CrouchLimit = int.Parse(p_value) * 0.01f;
                        CrouchLimitChange?.Invoke(CrouchLimit);
                    }
                    break;

                    case ModSetting.ProneLimit:
                    {
                        ProneLimit = int.Parse(p_value) * 0.01f;
                        ProneLimitChange?.Invoke(ProneLimit);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = int.Parse(p_value);
            }
        }

        static void OnToggleUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.IKOverrideCrouch:
                    {
                        IKOverrideCrouch = bool.Parse(p_value);
                        IKOverrideCrouchChange?.Invoke(IKOverrideCrouch);
                    }
                    break;

                    case ModSetting.IKOverrideProne:
                    {
                        IKOverrideProne = bool.Parse(p_value);
                        IKOverrideProneChange?.Invoke(IKOverrideProne);
                    }
                    break;

                    case ModSetting.PoseTransitions:
                    {
                        PoseTransitions = bool.Parse(p_value);
                        PoseTransitionsChange?.Invoke(PoseTransitions);
                    }
                    break;

                    case ModSetting.AdjustedMovement:
                    {
                        AdjustedMovement = bool.Parse(p_value);
                        AdjustedMovementChange?.Invoke(AdjustedMovement);
                    }
                    break;

                    case ModSetting.IKOverrideFly:
                    {
                        IKOverrideFly = bool.Parse(p_value);
                        IKOverrideFlyChange?.Invoke(IKOverrideFly);
                    }
                    break;

                    case ModSetting.IKOverrideJump:
                    {
                        IKOverrideJump = bool.Parse(p_value);
                        IKOverrideJumpChange?.Invoke(IKOverrideJump);
                    }
                    break;

                    case ModSetting.DetectEmotes:
                    {
                        DetectEmotes = bool.Parse(p_value);
                        DetectEmotesChange?.Invoke(DetectEmotes);
                    }
                    break;

                    case ModSetting.FollowHips:
                    {
                        FollowHips = bool.Parse(p_value);
                        FollowHipsChange?.Invoke(FollowHips);
                    }
                    break;

                    case ModSetting.MassCenter:
                    {
                        MassCenter = bool.Parse(p_value);
                        MassCenterChange?.Invoke(MassCenter);
                    }
                    break;

                    case ModSetting.ScaledSteps:
                    {
                        ScaledSteps = bool.Parse(p_value);
                        ScaledStepsChange?.Invoke(ScaledSteps);
                    }
                    break;

                    case ModSetting.ScaledJump:
                    {
                        ScaledJump = bool.Parse(p_value);
                        ScaledJumpChange?.Invoke(ScaledJump);
                    }
                    break;

                    case ModSetting.CollisionScale:
                    {
                        CollisionScale = bool.Parse(p_value);
                        CollisionScaleChange?.Invoke(CollisionScale);
                    }
                    break;

                    case ModSetting.OverrideFix:
                    {
                        OverrideFix = bool.Parse(p_value);
                        OverrideFixChange?.Invoke(OverrideFix);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = bool.Parse(p_value);
            }
        }
    }
}

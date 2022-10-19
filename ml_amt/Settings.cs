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
            DetectEmotes
        };

        static bool ms_ikOverrideCrouch = true;
        static float ms_crouchLimit = 0.65f;
        static bool ms_ikOverrideProne = true;
        static float ms_proneLimit = 0.3f;
        static bool ms_poseTransitions = true;
        static bool ms_adjustedMovement = true;
        static bool ms_ikOverrideFly = true;
        static bool ms_ikOverrideJump = true;
        static bool ms_detectEmotes = true;

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

        public static void Init()
        {
            ms_category = MelonLoader.MelonPreferences.CreateCategory("AMT");

            ms_entries = new List<MelonLoader.MelonPreferences_Entry>();
            ms_entries.Add(ms_category.CreateEntry(ModSetting.IKOverrideCrouch.ToString(), true));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.CrouchLimit.ToString(), 65));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.IKOverrideProne.ToString(), true));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.ProneLimit.ToString(), 30));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.PoseTransitions.ToString(), true));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.AdjustedMovement.ToString(), true));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.IKOverrideFly.ToString(), true));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.IKOverrideJump.ToString(), true));
            ms_entries.Add(ms_category.CreateEntry(ModSetting.DetectEmotes.ToString(), true));

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

        static void Load()
        {
            ms_ikOverrideCrouch = (bool)ms_entries[(int)ModSetting.IKOverrideCrouch].BoxedValue;
            ms_crouchLimit = ((int)ms_entries[(int)ModSetting.CrouchLimit].BoxedValue) * 0.01f;
            ms_ikOverrideProne = (bool)ms_entries[(int)ModSetting.IKOverrideProne].BoxedValue;
            ms_proneLimit = ((int)ms_entries[(int)ModSetting.ProneLimit].BoxedValue) * 0.01f;
            ms_poseTransitions = (bool)ms_entries[(int)ModSetting.PoseTransitions].BoxedValue;
            ms_adjustedMovement = (bool)ms_entries[(int)ModSetting.AdjustedMovement].BoxedValue;
            ms_ikOverrideFly = (bool)ms_entries[(int)ModSetting.IKOverrideFly].BoxedValue;
            ms_ikOverrideJump = (bool)ms_entries[(int)ModSetting.IKOverrideJump].BoxedValue;
            ms_detectEmotes = (bool)ms_entries[(int)ModSetting.DetectEmotes].BoxedValue;
        }

        static void OnSliderUpdate(string p_name, string p_value)
        {
            if(Enum.TryParse(p_name, out ModSetting l_setting))
            {
                switch(l_setting)
                {
                    case ModSetting.CrouchLimit:
                    {
                        ms_crouchLimit = int.Parse(p_value) * 0.01f;
                        CrouchLimitChange?.Invoke(ms_crouchLimit);
                    }
                    break;

                    case ModSetting.ProneLimit:
                    {
                        ms_proneLimit = int.Parse(p_value) * 0.01f;
                        ProneLimitChange?.Invoke(ms_proneLimit);
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
                        ms_ikOverrideCrouch = bool.Parse(p_value);
                        IKOverrideCrouchChange?.Invoke(ms_ikOverrideCrouch);
                    }
                    break;

                    case ModSetting.IKOverrideProne:
                    {
                        ms_ikOverrideProne = bool.Parse(p_value);
                        IKOverrideProneChange?.Invoke(ms_ikOverrideProne);
                    }
                    break;

                    case ModSetting.PoseTransitions:
                    {
                        ms_poseTransitions = bool.Parse(p_value);
                        PoseTransitionsChange?.Invoke(ms_poseTransitions);
                    }
                    break;

                    case ModSetting.AdjustedMovement:
                    {
                        ms_adjustedMovement = bool.Parse(p_value);
                        AdjustedMovementChange?.Invoke(ms_adjustedMovement);
                    }
                    break;

                    case ModSetting.IKOverrideFly:
                    {
                        ms_ikOverrideFly = bool.Parse(p_value);
                        IKOverrideFlyChange?.Invoke(ms_ikOverrideFly);
                    }
                    break;

                    case ModSetting.IKOverrideJump:
                    {
                        ms_ikOverrideJump = bool.Parse(p_value);
                        IKOverrideJumpChange?.Invoke(ms_ikOverrideJump);
                    }
                    break;

                    case ModSetting.DetectEmotes:
                    {
                        ms_detectEmotes = bool.Parse(p_value);
                        DetectEmotesChange?.Invoke(ms_detectEmotes);
                    }
                    break;
                }

                ms_entries[(int)l_setting].BoxedValue = bool.Parse(p_value);
            }
        }

        public static bool IKOverrideCrouch
        {
            get => ms_ikOverrideCrouch;
        }
        public static float CrouchLimit
        {
            get => ms_crouchLimit;
        }
        public static bool IKOverrideProne
        {
            get => ms_ikOverrideProne;
        }
        public static float ProneLimit
        {
            get => ms_proneLimit;
        }
        public static bool PoseTransitions
        {
            get => ms_poseTransitions;
        }
        public static bool AdjustedMovement
        {
            get => ms_adjustedMovement;
        }
        public static bool IKOverrideFly
        {
            get => ms_ikOverrideFly;
        }
        public static bool IKOverrideJump
        {
            get => ms_ikOverrideJump;
        }
        public static bool DetectEmotes
        {
            get => ms_detectEmotes;
        }
    }
}

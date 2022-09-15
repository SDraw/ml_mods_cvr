using ABI_RC.Core.Player;

namespace ml_amt
{
    public class AvatarMotionTweaker : MelonLoader.MelonMod
    {
        static AvatarMotionTweaker ms_instance = null;

        MotionTweaker m_localTweaker = null;

        public override void OnApplicationStart()
        {
            ms_instance = this;

            Settings.Init();
            Settings.IKOverrideCrouchChange += this.OnIKOverrideCrouchChange;
            Settings.CrouchLimitChange += this.OnCrouchLimitChange;
            Settings.IKOverrideProneChange += this.OnIKOverrideProneChange;
            Settings.ProneLimitChange += this.OnProneLimitChange;
            Settings.PoseTransitionsChange += this.OnPoseTransitonsChange;
            Settings.AdjustedMovementChange += this.OnAdjustedMovementChange;
            Settings.IKOverrideFlyChange += this.OnIKOverrideFlyChange;

            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnAvatarClear_Postfix), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod("SetupAvatarGeneral", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnSetupAvatarGeneral_Postfix), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
            );

            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
        }

        System.Collections.IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            m_localTweaker = PlayerSetup.Instance.gameObject.AddComponent<MotionTweaker>();
            m_localTweaker.SetIKOverrideCrouch(Settings.IKOverrideCrouch);
            m_localTweaker.SetCrouchLimit(Settings.CrouchLimit);
            m_localTweaker.SetIKOverrideCrouch(Settings.IKOverrideProne);
            m_localTweaker.SetProneLimit(Settings.ProneLimit);
            m_localTweaker.SetPoseTransitions(Settings.PoseTransitions);
            m_localTweaker.SetAdjustedMovement(Settings.AdjustedMovement);
            m_localTweaker.SetIKOverrideFly(Settings.IKOverrideFly);
        }
        
        void OnIKOverrideCrouchChange(bool p_state)
        {
            if(m_localTweaker != null)
                m_localTweaker.SetIKOverrideCrouch(p_state);
        }
        void OnCrouchLimitChange(float p_value)
        {
            if(m_localTweaker != null)
                m_localTweaker.SetCrouchLimit(p_value);
        }
        void OnIKOverrideProneChange(bool p_state)
        {
            if(m_localTweaker != null)
                m_localTweaker.SetIKOverrideProne(p_state);
        }
        void OnProneLimitChange(float p_value)
        {
            if(m_localTweaker != null)
                m_localTweaker.SetProneLimit(p_value);
        }
        void OnPoseTransitonsChange(bool p_state)
        {
            if(m_localTweaker != null)
                m_localTweaker.SetPoseTransitions(p_state);
        }
        void OnAdjustedMovementChange(bool p_state)
        {
            if(m_localTweaker != null)
                m_localTweaker.SetAdjustedMovement(p_state);
        }
        void OnIKOverrideFlyChange(bool p_state)
        {
            if(m_localTweaker != null)
                m_localTweaker.SetIKOverrideFly(p_state);
        }

        static void OnAvatarClear_Postfix() => ms_instance?.OnAvatarClear();
        void OnAvatarClear()
        {
            try
            {
                if(m_localTweaker != null)
                    m_localTweaker.OnAvatarClear();
            }
            catch(System.Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }

        static void OnSetupAvatarGeneral_Postfix() => ms_instance?.OnSetupAvatarGeneral();
        void OnSetupAvatarGeneral()
        {
            try
            {
                if(m_localTweaker != null)
                    m_localTweaker.OnSetupAvatarGeneral();
            }
            catch(System.Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }
    }
}

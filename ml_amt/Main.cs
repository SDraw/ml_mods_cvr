using ABI_RC.Core.Player;
using System.Reflection;

namespace ml_amt
{
    public class AvatarMotionTweaker : MelonLoader.MelonMod
    {
        static AvatarMotionTweaker ms_instance = null;

        MotionTweaker m_localTweaker = null;

        public override void OnInitializeMelon()
        {
            if(ms_instance == null)
                ms_instance = this;

            Settings.Init();
            Settings.IKOverrideCrouchChange += this.OnIKOverrideCrouchChange;
            Settings.CrouchLimitChange += this.OnCrouchLimitChange;
            Settings.IKOverrideProneChange += this.OnIKOverrideProneChange;
            Settings.ProneLimitChange += this.OnProneLimitChange;
            Settings.PoseTransitionsChange += this.OnPoseTransitonsChange;
            Settings.AdjustedMovementChange += this.OnAdjustedMovementChange;
            Settings.IKOverrideFlyChange += this.OnIKOverrideFlyChange;
            Settings.DetectEmotesChange += this.OnDetectEmotesChange;

            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnAvatarClear_Postfix), BindingFlags.NonPublic | BindingFlags.Static))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.CalibrateAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnCalibrateAvatar_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
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
            m_localTweaker.SetDetectEmotes(Settings.DetectEmotes);
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
        void OnDetectEmotesChange(bool p_state)
        {
            if(m_localTweaker != null)
                m_localTweaker.SetDetectEmotes(p_state);
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

        static void OnCalibrateAvatar_Postfix() => ms_instance?.OnCalibrateAvatar();
        void OnCalibrateAvatar()
        {
            try
            {
                if(m_localTweaker != null)
                    m_localTweaker.OnCalibrateAvatar();
            }
            catch(System.Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }
    }
}

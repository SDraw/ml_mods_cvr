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
            Settings.CrouchLimitChange += this.OnCrouchLimitChange;

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
        }

        void OnCrouchLimitChange(float p_value)
        {
            if(m_localTweaker != null)
                m_localTweaker.SetCrouchLimit(p_value);
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

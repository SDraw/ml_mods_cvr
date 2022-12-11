using ABI_RC.Core.Player;
using System.Reflection;

namespace ml_amt
{
    public class AvatarMotionTweaker : MelonLoader.MelonMod
    {
        static AvatarMotionTweaker ms_instance = null;

        MotionTweaker m_localTweaker = null;

        static int ms_calibrationCounts = 0;

        public override void OnInitializeMelon()
        {
            if(ms_instance == null)
                ms_instance = this;

            Settings.Init();

            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnAvatarClear_Postfix), BindingFlags.NonPublic | BindingFlags.Static))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetupAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnSetupAvatar_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(ABI_RC.Systems.IK.SubSystems.BodySystem).GetMethod(nameof(ABI_RC.Systems.IK.SubSystems.BodySystem.Calibrate)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnCalibrate_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(ABI_RC.Systems.IK.SubSystems.BodySystem).GetMethod(nameof(ABI_RC.Systems.IK.SubSystems.BodySystem.FBTAvailable)),
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnFBTAvailable_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                null
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ReCalibrateAvatar)),
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnReCalibrateAvatar_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                null
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
            m_localTweaker.SetIKOverrideJump(Settings.IKOverrideJump);
            m_localTweaker.SetDetectEmotes(Settings.DetectEmotes);
            m_localTweaker.SetFollowHips(Settings.FollowHips);
        }

        public override void OnDeinitializeMelon()
        {
            if(ms_instance == this)
                ms_instance = null;

            m_localTweaker = null;
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

        static void OnSetupAvatar_Postfix() => ms_instance?.OnSetupAvatar();
        void OnSetupAvatar()
        {
            try
            {
                ms_calibrationCounts = 0;

                if(m_localTweaker != null)
                    m_localTweaker.OnSetupAvatar();
            }
            catch(System.Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }

        static void OnCalibrate_Postfix() => ms_instance?.OnCalibrate();
        void OnCalibrate()
        {
            try
            {
                if(m_localTweaker != null)
                    m_localTweaker.OnCalibrate();
            }
            catch(System.Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }

        static void OnReCalibrateAvatar_Prefix()
        {
            MotionTweaker.ms_fptActive = false;
            ms_calibrationCounts++;
        }

        static bool OnFBTAvailable_Prefix(ref bool __result)
        {
            if(MotionTweaker.ms_fptActive || (ms_calibrationCounts == 0))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}

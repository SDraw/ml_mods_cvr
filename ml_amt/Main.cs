using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Systems.IK.SubSystems;
using System;
using System.Collections;
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
                typeof(BodySystem).GetMethod(nameof(BodySystem.Calibrate)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnCalibrate_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod("SetPlaySpaceScale", BindingFlags.NonPublic | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnPlayspaceScale_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            

            // Fixes
            Fixes.AnimatorOverrideControllerFix.Init(HarmonyInstance);
            Fixes.FBTDetectionFix.Init(HarmonyInstance);
            Fixes.PlayerColliderFix.Init(HarmonyInstance);
            Fixes.MovementJumpFix.Init(HarmonyInstance);

            ModSupporter.Init();
            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
        }

        IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            m_localTweaker = PlayerSetup.Instance.gameObject.AddComponent<MotionTweaker>();
            m_localTweaker.SetIKOverrideCrouch(Settings.IKOverrideCrouch);
            m_localTweaker.SetCrouchLimit(Settings.CrouchLimit);
            m_localTweaker.SetIKOverrideProne(Settings.IKOverrideProne);
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
            catch(Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }

        static void OnSetupAvatar_Postfix() => ms_instance?.OnSetupAvatar();
        void OnSetupAvatar()
        {
            try
            {
                if(m_localTweaker != null)
                    m_localTweaker.OnSetupAvatar();
            }
            catch(Exception l_exception)
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
            catch(Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }

        static void OnPlayspaceScale_Postfix() => ms_instance?.OnPlayspaceScale();
        void OnPlayspaceScale()
        {
            try
            {
                if(m_localTweaker != null)
                    m_localTweaker.OnPlayspaceScale();
            }
            catch(Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }
    }
}

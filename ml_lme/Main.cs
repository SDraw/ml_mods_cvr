using ABI_RC.Core.Player;
using ABI_RC.Systems.IK.SubSystems;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace ml_lme
{

    public class LeapMotionExtension : MelonLoader.MelonMod
    {
        static LeapMotionExtension ms_instance = null;

        LeapManager m_leapManager = null;

        public override void OnInitializeMelon()
        {
            if(ms_instance == null)
                ms_instance = this;

            DependenciesHandler.ExtractDependencies();
            Settings.Init();
            AssetsHandler.Load();

            // Patches
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(LeapMotionExtension).GetMethod(nameof(OnAvatarClear_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetupAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(LeapMotionExtension).GetMethod(nameof(OnSetupAvatar_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(BodySystem).GetMethod(nameof(BodySystem.Calibrate)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(LeapMotionExtension).GetMethod(nameof(OnCalibrate_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetControllerRayScale)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(LeapMotionExtension).GetMethod(nameof(OnRayScale_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod("SetPlaySpaceScale", BindingFlags.NonPublic | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(LeapMotionExtension).GetMethod(nameof(OnPlayspaceScale_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );

            ModSupporter.Init();
            MelonLoader.MelonCoroutines.Start(WaitForRootLogic());
        }

        public override void OnDeinitializeMelon()
        {
            if(ms_instance == this)
                ms_instance = null;
        }

        IEnumerator WaitForRootLogic()
        {
            while(ABI_RC.Core.RootLogic.Instance == null)
                yield return null;

            m_leapManager = new GameObject("LeapMotionManager").AddComponent<LeapManager>();
        }

        // Patches
        static void OnAvatarClear_Postfix() => ms_instance?.OnAvatarClear();
        void OnAvatarClear()
        {
            try
            {
                if(m_leapManager != null)
                    m_leapManager.OnAvatarClear();
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnSetupAvatar_Postfix() => ms_instance?.OnSetupAvatar();
        void OnSetupAvatar()
        {
            try
            {
                if(m_leapManager != null)
                    m_leapManager.OnAvatarSetup();
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnCalibrate_Postfix() => ms_instance?.OnCalibrate();
        void OnCalibrate()
        {
            try
            {
                if(m_leapManager != null)
                    m_leapManager.OnCalibrate();
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnRayScale_Postfix(float __0) => ms_instance?.OnRayScale(__0);
        void OnRayScale(float p_scale)
        {
            try
            {
                if(m_leapManager != null)
                    m_leapManager.OnRayScale(p_scale);
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnPlayspaceScale_Postfix(float ____avatarScaleRelation) => ms_instance?.OnPlayspaceScale(____avatarScaleRelation);
        void OnPlayspaceScale(float p_relation)
        {
            try
            {
                if(m_leapManager != null)
                    m_leapManager.OnPlayspaceScale(p_relation);
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

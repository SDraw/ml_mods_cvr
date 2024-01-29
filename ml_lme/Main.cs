using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Systems.IK;
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
                typeof(IKSystem).GetMethod(nameof(IKSystem.ReinitializeAvatar), BindingFlags.Instance | BindingFlags.Public),
                null,
                new HarmonyLib.HarmonyMethod(typeof(LeapMotionExtension).GetMethod(nameof(OnAvatarReinitialize_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
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
            HarmonyInstance.Patch(

                typeof(CVRPickupObject).GetMethod(nameof(CVRPickupObject.Grab), BindingFlags.Public | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(LeapMotionExtension).GetMethod(nameof(OnPickupGrab_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );

            ModSupporter.Init();
            MelonLoader.MelonCoroutines.Start(WaitForRootLogic());
        }

        public override void OnDeinitializeMelon()
        {
            if(ms_instance == this)
                ms_instance = null;

            if(m_leapManager != null)
                Object.Destroy(m_leapManager);
            m_leapManager = null;
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

        static void OnAvatarReinitialize_Postfix() => ms_instance?.OnAvatarReinitialize();
        void OnAvatarReinitialize()
        {
            try
            {
                if(m_leapManager != null)
                    m_leapManager.OnAvatarReinitialize();
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

        static void OnPickupGrab_Postfix(ref CVRPickupObject __instance) => ms_instance?.OnPickupGrab(__instance);
        void OnPickupGrab(CVRPickupObject p_pickup)
        {
            try
            {
                if(m_leapManager != null)
                    m_leapManager.OnPickupGrab(p_pickup);
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

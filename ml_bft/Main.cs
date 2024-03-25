using ABI_RC.Core.Player;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.InputManagement;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace ml_bft
{
    public class BetterFingersTracking : MelonLoader.MelonMod
    {
        static BetterFingersTracking ms_instance = null;

        InputHandler m_inputHandler = null;
        FingerSystem m_fingerSystem = null;

        public override void OnInitializeMelon()
        {
            if(ms_instance == null)
                ms_instance = this;

            Settings.Init();
            AssetsHandler.Load();

            // Needed patches: avatar initialization and reinitialization on vr switch, after input update, after late iksystem update
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetupAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(BetterFingersTracking).GetMethod(nameof(OnSetupAvatar_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(BetterFingersTracking).GetMethod(nameof(OnAvatarClear_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(IKSystem).GetMethod(nameof(IKSystem.ReinitializeAvatar), BindingFlags.Instance | BindingFlags.Public),
                null,
                new HarmonyLib.HarmonyMethod(typeof(BetterFingersTracking).GetMethod(nameof(OnReinitializeAvatar_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(CVRInputManager).GetMethod("UpdateInput", BindingFlags.NonPublic | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(BetterFingersTracking).GetMethod(nameof(OnInputUpdate_Postfix), BindingFlags.NonPublic | BindingFlags.Static))
            );
            HarmonyInstance.Patch(
                typeof(IKSystem).GetMethod("LateUpdate", BindingFlags.NonPublic | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(BetterFingersTracking).GetMethod(nameof(OnIKSystemLateUpdate_Postfix), BindingFlags.NonPublic | BindingFlags.Static))
            );

            MelonLoader.MelonCoroutines.Start(WaitForInstances());
        }
        IEnumerator WaitForInstances()
        {
            while(CVRInputManager.Instance == null)
                yield return null;

            m_inputHandler = new InputHandler();
            m_fingerSystem = new FingerSystem();
        }

        public override void OnDeinitializeMelon()
        {
            if(ms_instance == this)
                ms_instance = null;

            m_inputHandler?.Cleanup();
            m_inputHandler = null;

            m_fingerSystem?.Cleanup();
            m_fingerSystem = null;
        }

        static void OnSetupAvatar_Postfix() => ms_instance?.OnSetupAvatar();
        void OnSetupAvatar()
        {
            try
            {
                m_fingerSystem?.OnAvatarSetup();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnAvatarClear_Postfix() => ms_instance?.OnAvatarClear();
        void OnAvatarClear()
        {
            try
            {
                m_fingerSystem?.OnAvatarClear();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnReinitializeAvatar_Postfix() => ms_instance?.OnAvatarReinitialize();
        void OnAvatarReinitialize()
        {
            try
            {
                m_fingerSystem?.OnReinitializeAvatar();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnInputUpdate_Postfix() => ms_instance?.OnInputUpdate();
        void OnInputUpdate()
        {
            try
            {
                m_inputHandler?.OnInputUpdate();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnIKSystemLateUpdate_Postfix(HumanPoseHandler ____humanPoseHandler) => ms_instance?.OnIKSystemLateUpdate(____humanPoseHandler);
        void OnIKSystemLateUpdate(HumanPoseHandler p_handler)
        {
            try
            {
                m_fingerSystem?.OnIKSystemLateUpdate(p_handler);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

using ABI_RC.Core;
using ABI_RC.Core.Player;
using ABI_RC.Core.Player.EyeMovement;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.FaceTracking;
using ABI_RC.Systems.FaceTracking.Impl;
using System.Reflection;
using System.Threading;
using UnityEngine;
using ViveSR.anipal.Eye;

namespace ml_vet
{
    public class ViveEyeTracking : MelonLoader.MelonMod
    {
        const string c_gameSettingName = "ImplementationVRViveFaceTracking";

        static ViveEyeTracking ms_instance = null;

        readonly object m_lock = new object();
        EyeData_v2 m_threadEyeData; // Shared between threads
        SingleEyeData m_combinedEyeData;
        SingleEyeData m_leftEyeData;
        SingleEyeData m_rightEyeData;

        Vector2 m_openness = Vector2.one;
        Vector3 m_gazeDirection = Vector3.forward;

        bool m_faceTrackingEnabled = false;

        public override void OnInitializeMelon()
        {
            ms_instance = this;

            Settings.Init();

            HarmonyInstance.Patch(
                typeof(SRanipalTrackingModule).GetMethod(nameof(SRanipalTrackingModule.Initialize), BindingFlags.Public | BindingFlags.Instance),
                new HarmonyLib.HarmonyMethod(typeof(ViveEyeTracking).GetMethod(nameof(SRanipalTrackingModuleUpdate_Prefix), BindingFlags.NonPublic | BindingFlags.Static))
            );
            HarmonyInstance.Patch(typeof(EyeMovementController).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(ViveEyeTracking).GetMethod(nameof(EyeMovementControllerUpdate_Postfix), BindingFlags.NonPublic | BindingFlags.Static))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod("UpdatePlayerAvatarMovementData", BindingFlags.Instance | BindingFlags.NonPublic),
                null,
                new HarmonyLib.HarmonyMethod(typeof(ViveEyeTracking).GetMethod(nameof(OnPlayerAvatarMovementDataUpdate_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );

            MelonLoader.MelonCoroutines.Start(WaitForRootLogic(HarmonyInstance));
        }

        System.Collections.IEnumerator WaitForRootLogic(HarmonyLib.Harmony p_harmony)
        {
            while(RootLogic.Instance == null)
                yield return null;
            while(MetaPort.Instance == null)
                yield return null;

            p_harmony.Patch(
                typeof(FaceTrackingManager).GetMethod(nameof(FaceTrackingManager.SubmitNewEyeData), BindingFlags.Public | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(ViveEyeTracking).GetMethod(nameof(FaceTrackingManagerSubmitNewEyeData_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );

            m_faceTrackingEnabled = MetaPort.Instance.settings.GetSettingsBool(c_gameSettingName);
            MetaPort.Instance.settings.settingBoolChanged.AddListener(this.OnGameSettingBoolChanged);
        }

        public override void OnDeinitializeMelon()
        {
            ms_instance = null;
        }

        public override void OnUpdate()
        {
            if(Settings.Enabled && m_faceTrackingEnabled && Utils.IsInVR())
            {
                if(Monitor.TryEnter(m_lock))
                {
                    m_combinedEyeData = m_threadEyeData.verbose_data.combined.eye_data;
                    m_leftEyeData = m_threadEyeData.verbose_data.left;
                    m_rightEyeData = m_threadEyeData.verbose_data.right;
                    Monitor.Exit(m_lock);
                }

                if(m_combinedEyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY))
                {
                    m_gazeDirection = m_combinedEyeData.gaze_direction_normalized;
                    m_gazeDirection.x *= -1f;
                }

                if(m_leftEyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY))
                    m_openness.x = m_leftEyeData.eye_openness;

                if(m_rightEyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY))
                    m_openness.y = m_rightEyeData.eye_openness;
            }
        }

        void OnGameSettingBoolChanged(string p_name, bool p_state)
        {
            if(p_name == c_gameSettingName)
                m_faceTrackingEnabled = p_state;
        }

        bool IsReadyForUpdate() => (Settings.Enabled && m_faceTrackingEnabled && FaceTrackingManager.Instance.IsEyeDataAvailable());

        // Patches
        static void SRanipalTrackingModuleUpdate_Prefix(ref bool useEye, bool useLip)
        {
            // Hijack face tracking module to be eye tracking module as well, because it is SRanipal too and can initialize without issues
            if(useLip && Settings.Enabled)
                useEye = true;
        }

        static void FaceTrackingManagerSubmitNewEyeData_Postfix(ref EyeData_v2 eyeData) => ms_instance?.OnEyeDataPostSubmit(ref eyeData);
        void OnEyeDataPostSubmit(ref EyeData_v2 p_data)
        {
            // This is called not in main thread
            try
            {
                Monitor.Enter(m_lock);
                m_threadEyeData = p_data;
                Monitor.Exit(m_lock);
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void EyeMovementControllerUpdate_Postfix(ref EyeMovementController __instance) => ms_instance?.OnEyeMovementControllerPostUpdate(__instance);
        void OnEyeMovementControllerPostUpdate(EyeMovementController p_controller)
        {
            try
            {
                if(p_controller.IsLocal && IsReadyForUpdate())
                {
                    p_controller.manualBlinking = true;
                    p_controller.blinkProgress = 1f - Mathf.Clamp01((m_openness.x + m_openness.y) * 0.5f);

                    p_controller.manualViewTarget = true;
                    p_controller.targetViewPosition = p_controller.ViewPointTransform.position + p_controller.ViewPointTransform.rotation * (m_gazeDirection * 5f);
                }
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnPlayerAvatarMovementDataUpdate_Postfix(ref PlayerSetup __instance, PlayerAvatarMovementData ____playerAvatarMovementData) => ms_instance?.OnPlayerAvatarMovementDataPostUpdate(__instance, ____playerAvatarMovementData);
        void OnPlayerAvatarMovementDataPostUpdate(PlayerSetup p_instance, PlayerAvatarMovementData p_data)
        {
            try
            {
                if(IsReadyForUpdate() && (p_instance.EyeMovementController != null))
                {
                    p_data.EyeTrackingBlinkProgressLeft = 1f - Mathf.Clamp01(m_openness.x);
                    p_data.EyeTrackingBlinkProgressRight = 1f - Mathf.Clamp01(m_openness.y);
                }
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

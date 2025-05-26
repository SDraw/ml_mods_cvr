using ABI_RC.Core;
using ABI_RC.Core.Player;
using ABI_RC.Core.Player.EyeMovement;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.FaceTracking;
using ABI_RC.Systems.FaceTracking.Impl;
using ABI_RC.Systems.RuntimeDebug;
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
        Vector3 m_leftEyeGaze = new Vector3(1f, 0f, 1f).normalized;
        Vector3 m_rightEyeGaze = new Vector3(-1f, 0f, 1f).normalized;
        Vector3 m_combinedGaze = Vector3.forward;
        Vector3 m_leftEyeOrigin = new Vector3(-0.025f, 0f, 0f);
        Vector3 m_rightEyeOrigin = new Vector3(0.025f, 0f, 0f);
        Vector3 m_cominedGazeOrigin = Vector3.zero;
        bool m_leftEyeGazeValid = false;
        bool m_rightEyeGazeValid = false;
        bool m_combinedGazeValid = false;
        Vector3 m_gazePoint = Vector3.forward;
        Vector3 m_gazeVelocity = Vector3.zero;

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

                m_combinedGazeValid = m_combinedEyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY);
                if(m_combinedGazeValid)
                {
                    m_combinedGaze = m_combinedEyeData.gaze_direction_normalized;
                    m_combinedGaze.x *= -1f;
                }
                if(m_combinedEyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY))
                {
                    m_cominedGazeOrigin = m_combinedEyeData.gaze_origin_mm * 0.001f;
                    m_cominedGazeOrigin.x *= -1f;
                }


                m_leftEyeGazeValid = m_leftEyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY);
                if(m_leftEyeGazeValid)
                {
                    m_leftEyeGaze = m_leftEyeData.gaze_direction_normalized;
                    m_leftEyeGaze.x *= -1f;
                }
                if(m_leftEyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY))
                {
                    m_leftEyeOrigin = m_leftEyeData.gaze_origin_mm * 0.001f;
                    m_leftEyeOrigin.x *= -1f;
                }
                if(m_leftEyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY))
                    m_openness.x = m_leftEyeData.eye_openness;

                m_rightEyeGazeValid = m_rightEyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY);
                if(m_rightEyeGazeValid)
                {
                    m_rightEyeGaze = m_rightEyeData.gaze_direction_normalized;
                    m_rightEyeGaze.x *= -1f;
                }
                if(m_rightEyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY))
                {
                    m_rightEyeOrigin = m_rightEyeData.gaze_origin_mm * 0.001f;
                    m_rightEyeOrigin.x *= -1f;
                }
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
                    p_controller.blinkProgressLeft = 1f - m_openness.x;
                    p_controller.blinkProgressRight = 1f - m_openness.y;

                    p_controller.manualViewTarget = true;

                    Vector3 l_gazePoint = m_gazePoint;
                    float l_playspaceScale = PlayerSetup.Instance.GetPlaySpaceScale();

                    if(m_leftEyeGazeValid && m_rightEyeGazeValid)
                    {
                        // https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
                        // Projecting on OXZ plane to intersect eyes
                        float x1 = m_leftEyeOrigin.x;
                        float x2 = m_leftEyeOrigin.x + m_leftEyeGaze.x;
                        float y1 = m_leftEyeOrigin.z;
                        float y2 = m_leftEyeOrigin.z + m_leftEyeGaze.z;

                        float x3 = m_rightEyeOrigin.x;
                        float x4 = m_rightEyeOrigin.x + m_rightEyeGaze.x;
                        float y3 = m_rightEyeOrigin.z;
                        float y4 = m_rightEyeOrigin.z + m_rightEyeGaze.z;

                        float l_det = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
                        Vector3 l_combineGazeOrigin = Vector3.Lerp(m_leftEyeOrigin, m_rightEyeOrigin, 0.5f); // SRanipal's combined gaze origin is unstable

                        if(!Mathf.Approximately(l_det, 0f))
                        {
                            float l_detZ = (x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4);
                            float l_posZ = l_detZ / l_det;

                            float l_thetaLeft = (l_posZ - m_leftEyeOrigin.z) / m_leftEyeGaze.z;
                            Vector3 l_leftEyePoint = m_leftEyeOrigin + l_thetaLeft * m_leftEyeGaze;

                            float l_thetaRight = (l_posZ - m_rightEyeOrigin.z) / m_rightEyeGaze.z;
                            Vector3 l_rightEyePoint = m_rightEyeOrigin + l_thetaRight * m_rightEyeGaze;

                            Vector3 l_midPoint = Vector3.Lerp(l_leftEyePoint, l_rightEyePoint, 0.5f);
                            if(l_midPoint.z > l_combineGazeOrigin.z)
                            {
                                Vector3 l_resultDir = l_midPoint - l_combineGazeOrigin;
                                l_resultDir = Vector3.ClampMagnitude(l_resultDir, 1f);
                                l_gazePoint = (l_combineGazeOrigin + l_resultDir) * l_playspaceScale;
                            }
                            else
                                l_gazePoint = (l_combineGazeOrigin + m_combinedGaze) * l_playspaceScale;
                        }
                        else
                            l_gazePoint = (l_combineGazeOrigin + m_combinedGaze) * l_playspaceScale;
                    }
                    else
                    {
                        if(m_leftEyeGazeValid)
                            l_gazePoint = (m_leftEyeOrigin + m_leftEyeGaze) * l_playspaceScale;
                        if(m_rightEyeGazeValid)
                            l_gazePoint = (m_rightEyeGaze + m_rightEyeGaze) * l_playspaceScale;
                    }

                    l_gazePoint = Vector3.SmoothDamp(m_gazePoint, l_gazePoint, ref m_gazeVelocity, Settings.Smoothing);
                    p_controller.targetViewPosition = p_controller.ViewPointTransform.position + p_controller.ViewPointTransform.rotation * l_gazePoint;

                    if(m_rightEyeGazeValid || m_leftEyeGazeValid)
                        m_gazePoint = l_gazePoint;

                    if(Settings.Debug)
                    {
                        Vector3 l_pos = p_controller.ViewPointTransform.position;
                        Quaternion l_rot = p_controller.ViewPointTransform.rotation;

                        RuntimeGizmos.DrawArrowFromTo(
                            l_pos + l_rot * (m_leftEyeOrigin * l_playspaceScale),
                            l_pos + l_rot * ((m_leftEyeOrigin + (m_leftEyeGazeValid ? m_leftEyeGaze : Vector3.forward)) * l_playspaceScale),
                            0.01f * l_playspaceScale, Color.blue, CVRLayers.Default, 0.25f
                        );
                        RuntimeGizmos.DrawArrowFromTo(
                            l_pos + l_rot * (m_rightEyeOrigin * l_playspaceScale),
                            l_pos + l_rot * ((m_rightEyeOrigin + (m_rightEyeGazeValid ? m_rightEyeGaze : Vector3.forward)) * l_playspaceScale),
                            0.01f * l_playspaceScale, Color.blue, CVRLayers.Default, 0.25f
                        );
                        RuntimeGizmos.DrawArrowFromTo(
                            l_pos + l_rot * (m_cominedGazeOrigin * l_playspaceScale),
                            l_pos + l_rot * ((m_cominedGazeOrigin + (m_combinedGazeValid ? m_combinedGaze : Vector3.forward)) * l_playspaceScale),
                            0.01f * l_playspaceScale, Color.red, CVRLayers.Default, 0.25f
                        );

                        RuntimeGizmos.DrawSphere(p_controller.targetViewPosition, 0.01f * l_playspaceScale, Color.cyan, CVRLayers.Default, 0.25f);
                    }
                }
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

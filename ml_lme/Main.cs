using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Core.UI;
using UnityEngine;

namespace ml_lme
{

    public class LeapMotionExtension : MelonLoader.MelonMod
    {
        static readonly Quaternion ms_hmdRotationFix = new Quaternion(0f, 0.7071068f, 0.7071068f, 0f);
        static readonly Quaternion ms_screentopRotationFix = new Quaternion(0f, 0f, -1f, 0f);

        static LeapMotionExtension ms_instance = null;

        Leap.Controller m_leapController = null;
        GestureMatcher.GesturesData m_gesturesData = null;

        GameObject m_leapTrackingRoot = null;
        GameObject[] m_leapHands = null;
        GameObject m_leapControllerModel = null;
        LeapTracked m_leapTracked = null;

        public override void OnApplicationStart()
        {
            if(ms_instance == null)
                ms_instance = this;

            DependenciesHandler.ExtractDependencies();

            Settings.Init();
            Settings.EnabledChange += this.OnSettingsEnableChange;
            Settings.DesktopOffsetChange += this.OnSettingsDesktopOffsetChange;
            Settings.FingersOnlyChange += this.OnSettingsFingersOptionChange;
            Settings.ModelVisibilityChange += this.OnSettingsModelVisibilityChange;
            Settings.TrackingModeChange += this.OnSettingsTrackingModeChange;
            Settings.RootAngleChange += this.OnSettingsRootAngleChange;
            Settings.HeadAttachChange += this.OnSettingsHeadAttachChange;
            Settings.HeadOffsetChange += this.OnSettingsHeadOffsetChange;

            m_leapController = new Leap.Controller();
            m_leapController.Device += this.OnLeapDeviceInitialized;
            m_leapController.DeviceFailure += this.OnLeapDeviceFailure;
            m_leapController.DeviceLost += this.OnLeapDeviceLost;
            m_leapController.Connect += this.OnLeapServiceConnect;
            m_leapController.Disconnect += this.OnLeapServiceDisconnect;

            m_gesturesData = new GestureMatcher.GesturesData();
            m_leapHands = new GameObject[GestureMatcher.GesturesData.ms_handsCount];

            // Patches
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetupAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(LeapMotionExtension).GetMethod(nameof(OnAvatarSetup_Postfix), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(LeapMotionExtension).GetMethod(nameof(OnAvatarClear_Postfix), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
            );

            MelonLoader.MelonCoroutines.Start(CreateTrackingObjects());
        }

        System.Collections.IEnumerator CreateTrackingObjects()
        {
            AssetsHandler.Load();

            while(PlayerSetup.Instance == null)
                yield return null;
            while(PlayerSetup.Instance.desktopCameraRig == null)
                yield return null;
            while(PlayerSetup.Instance.desktopCamera == null)
                yield return null;
            while(PlayerSetup.Instance.vrCameraRig == null)
                yield return null;
            while(PlayerSetup.Instance.vrCamera == null)
                yield return null;

            m_leapTrackingRoot = new GameObject("[LeapRoot]");

            for(int i = 0; i < GestureMatcher.GesturesData.ms_handsCount; i++)
            {
                m_leapHands[i] = new GameObject("LeapHand" + i);
                m_leapHands[i].transform.parent = m_leapTrackingRoot.transform;
                m_leapHands[i].transform.localPosition = Vector3.zero;
                m_leapHands[i].transform.localRotation = Quaternion.identity;
            }

            m_leapControllerModel = AssetsHandler.GetAsset("assets/models/leapmotion/leap_motion_1_0.obj");
            if(m_leapControllerModel != null)
            {
                m_leapControllerModel.name = "LeapModel";
                m_leapControllerModel.transform.parent = m_leapTrackingRoot.transform;
                m_leapControllerModel.transform.localPosition = Vector3.zero;
                m_leapControllerModel.transform.localRotation = Quaternion.identity;
            }

            OnSettingsEnableChange(Settings.Enabled);
            OnSettingsFingersOptionChange(Settings.FingersOnly);
            OnSettingsModelVisibilityChange(Settings.ModelVisibility);
            OnSettingsTrackingModeChange(Settings.TrackingMode);
            OnSettingsHeadAttachChange(Settings.HeadAttach); // Includes offsets and parenting
        }

        public override void OnUpdate()
        {
            if(Settings.Enabled)
            {
                for(int i = 0; i < GestureMatcher.GesturesData.ms_handsCount; i++)
                    m_gesturesData.m_handsPresenses[i] = false;

                if((m_leapController != null) && m_leapController.IsConnected)
                {
                    Leap.Frame l_frame = m_leapController.Frame();
                    if(l_frame != null)
                    {
                        GestureMatcher.GetGestures(l_frame, ref m_gesturesData);

                        for(int i = 0; i < GestureMatcher.GesturesData.ms_handsCount; i++)
                        {
                            if((m_leapHands[i] != null) && m_gesturesData.m_handsPresenses[i])
                            {
                                Vector3 l_pos = m_gesturesData.m_handsPositons[i];
                                Quaternion l_rot = m_gesturesData.m_handsRotations[i];
                                ReorientateLeapToUnity(ref l_pos, ref l_rot, Settings.TrackingMode);
                                m_leapHands[i].transform.localPosition = l_pos;
                                m_leapHands[i].transform.localRotation = l_rot;
                            }
                        }
                    }
                }

                if(m_leapTracked != null)
                    m_leapTracked.UpdateTracking(m_gesturesData);
            }
        }

        // Settings changes
        void OnSettingsEnableChange(bool p_state)
        {
            if(p_state)
            {
                m_leapController.StartConnection();
                UpdateDeviceTrackingMode();
            }
            else
                m_leapController.StopConnection();

            if(m_leapTracked != null)
                m_leapTracked.SetEnabled(p_state);
        }

        void OnSettingsDesktopOffsetChange(Vector3 p_offset)
        {
            if((m_leapTrackingRoot != null) && !Settings.HeadAttach)
            {
                if(!PlayerSetup.Instance._inVr)
                    m_leapTrackingRoot.transform.localPosition = p_offset * PlayerSetup.Instance.vrCameraRig.transform.localScale.x;
                else
                    m_leapTrackingRoot.transform.localPosition = p_offset;
            }
        }

        void OnSettingsFingersOptionChange(bool p_state)
        {
            if(m_leapTracked != null)
                m_leapTracked.SetFingersOnly(p_state);
        }

        void OnSettingsModelVisibilityChange(bool p_state)
        {
            if(m_leapControllerModel != null)
                m_leapControllerModel.SetActive(p_state);
        }

        void OnSettingsTrackingModeChange(Settings.LeapTrackingMode p_mode)
        {
            if(Settings.Enabled && (m_leapController != null))
                UpdateDeviceTrackingMode();

            if(m_leapControllerModel != null)
            {
                switch(p_mode)
                {
                    case Settings.LeapTrackingMode.Screentop:
                        m_leapControllerModel.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                        break;
                    case Settings.LeapTrackingMode.Desktop:
                        m_leapControllerModel.transform.localRotation = Quaternion.identity;
                        break;
                    case Settings.LeapTrackingMode.HMD:
                        m_leapControllerModel.transform.localRotation = Quaternion.Euler(270f, 180f, 0f);
                        break;
                }
            }
        }

        void OnSettingsRootAngleChange(float p_angle)
        {
            if(m_leapTrackingRoot != null)
                m_leapTrackingRoot.transform.localRotation = Quaternion.Euler(p_angle, 0f, 0f);
        }

        void OnSettingsHeadAttachChange(bool p_state)
        {
            if(m_leapTrackingRoot != null)
            {
                if(p_state)
                {
                    if(!PlayerSetup.Instance._inVr)
                    {
                        m_leapTrackingRoot.transform.parent = PlayerSetup.Instance.desktopCamera.transform;
                        m_leapTrackingRoot.transform.localPosition = Settings.HeadOffset * PlayerSetup.Instance.vrCameraRig.transform.localScale.x;
                        m_leapTrackingRoot.transform.localScale = PlayerSetup.Instance.vrCameraRig.transform.localScale;
                    }
                    else
                    {
                        m_leapTrackingRoot.transform.parent = PlayerSetup.Instance.vrCamera.transform;
                        m_leapTrackingRoot.transform.localPosition = Settings.HeadOffset;
                        m_leapTrackingRoot.transform.localScale = Vector3.one;
                    }
                }
                else
                {
                    if(!PlayerSetup.Instance._inVr)
                    {
                        m_leapTrackingRoot.transform.parent = PlayerSetup.Instance.desktopCameraRig.transform;
                        m_leapTrackingRoot.transform.localPosition = Settings.DesktopOffset * PlayerSetup.Instance.vrCameraRig.transform.localScale.x;
                        m_leapTrackingRoot.transform.localScale = PlayerSetup.Instance.vrCameraRig.transform.localScale;
                    }
                    else
                    {
                        m_leapTrackingRoot.transform.parent = PlayerSetup.Instance.vrCameraRig.transform;
                        m_leapTrackingRoot.transform.localPosition = Settings.DesktopOffset;
                        m_leapTrackingRoot.transform.localScale = Vector3.one;
                    }
                }

                m_leapTrackingRoot.transform.localRotation = Quaternion.Euler(Settings.RootAngle, 0f, 0f);
            }
        }

        void OnSettingsHeadOffsetChange(Vector3 p_offset)
        {
            if((m_leapTrackingRoot != null) && Settings.HeadAttach)
            {
                if(!PlayerSetup.Instance._inVr)
                    m_leapTrackingRoot.transform.localPosition = p_offset * PlayerSetup.Instance.vrCameraRig.transform.localScale.x;
                else
                    m_leapTrackingRoot.transform.localPosition = p_offset;
            }
        }

        // Internal utility
        void UpdateDeviceTrackingMode()
        {
            m_leapController.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP, null);
            m_leapController.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD, null);

            switch(Settings.TrackingMode)
            {
                case Settings.LeapTrackingMode.Screentop:
                    m_leapController.SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP, null);
                    break;
                case Settings.LeapTrackingMode.HMD:
                    m_leapController.SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD, null);
                    break;
            }
        }

        // Leap events
        void OnLeapDeviceInitialized(object p_sender, Leap.DeviceEventArgs p_args)
        {
            if(Settings.Enabled && (m_leapController != null))
                UpdateDeviceTrackingMode();

            if(CohtmlHud.Instance != null)
                CohtmlHud.Instance.ViewDropText("Leap Motion Extension", "Device initialized");
        }

        void OnLeapDeviceFailure(object p_sender, Leap.DeviceFailureEventArgs p_args)
        {
            if(CohtmlHud.Instance != null)
                CohtmlHud.Instance.ViewDropText("Leap Motion Extension", "Device failure, code " + p_args.ErrorCode + ": " + p_args.ErrorMessage);
        }

        void OnLeapDeviceLost(object p_sender, Leap.DeviceEventArgs p_args)
        {
            if(CohtmlHud.Instance != null)
                CohtmlHud.Instance.ViewDropText("Leap Motion Extension", "Device lost");
        }

        void OnLeapServiceConnect(object p_sender, Leap.ConnectionEventArgs p_args)
        {
            if(CohtmlHud.Instance != null)
                CohtmlHud.Instance.ViewDropText("Leap Motion Extension", "Service connected");
        }

        void OnLeapServiceDisconnect(object p_sender, Leap.ConnectionLostEventArgs p_args)
        {
            if(CohtmlHud.Instance != null)
                CohtmlHud.Instance.ViewDropText("Leap Motion Extension", "Service disconnected");
        }

        // Patches
        static void OnAvatarClear_Postfix(ref PlayerSetup __instance)
        {
            if((__instance != null) && (__instance == PlayerSetup.Instance))
                ms_instance?.OnAvatarClear();
        }
        void OnAvatarClear()
        {
            if(m_leapTracked != null)
            {
                Object.DestroyImmediate(m_leapTracked);
                m_leapTracked = null;
            }
        }

        static void OnAvatarSetup_Postfix(ref PlayerSetup __instance)
        {
            if((__instance != null) && (__instance == PlayerSetup.Instance))
                ms_instance?.OnAvatarSetup(__instance._animator, __instance.GetComponent<IndexIK>());
        }
        void OnAvatarSetup(Animator p_animator, IndexIK p_indexIK)
        {
            if(m_leapTracked == null)
            {
                m_leapTracked = p_indexIK.gameObject.AddComponent<LeapTracked>();
                m_leapTracked.SetEnabled(Settings.Enabled);
                m_leapTracked.SetAnimator(p_animator);
                m_leapTracked.SetHands(m_leapHands[0].transform, m_leapHands[1].transform);
                m_leapTracked.SetFingersOnly(Settings.FingersOnly);

                OnSettingsHeadAttachChange(Settings.HeadAttach);
            }
        }

        // Utilities
        static void ReorientateLeapToUnity(ref Vector3 p_pos, ref Quaternion p_rot, Settings.LeapTrackingMode p_mode)
        {
            p_pos *= 0.001f;
            p_pos.z *= -1f;
            p_rot.x *= -1f;
            p_rot.y *= -1f;

            switch(p_mode)
            {
                case Settings.LeapTrackingMode.Screentop:
                {
                    p_pos.x *= -1f;
                    p_pos.y *= -1f;
                    p_rot = (ms_screentopRotationFix * p_rot);
                }
                break;

                case Settings.LeapTrackingMode.HMD:
                {
                    p_pos.x *= -1f;
                    Utils.Swap(ref p_pos.y, ref p_pos.z);
                    p_rot = (ms_hmdRotationFix * p_rot);
                }
                break;
            }
        }
    }
}

using ABI_RC.Core.Player;
using ABI_RC.Core.UI;
using System.Reflection;
using UnityEngine;

namespace ml_lme
{

    public class LeapMotionExtension : MelonLoader.MelonMod
    {
        static LeapMotionExtension ms_instance = null;

        Leap.Controller m_leapController = null;
        GestureMatcher.GesturesData m_gesturesData = null;

        GameObject m_leapTrackingRoot = null;
        GameObject[] m_leapHands = null;
        GameObject[] m_leapElbows = null;
        GameObject m_leapControllerModel = null;
        LeapTracked m_leapTracked = null;

        bool m_isInVR = false;

        public override void OnInitializeMelon()
        {
            if(ms_instance == null)
                ms_instance = this;

            DependenciesHandler.ExtractDependencies();

            Settings.Init();
            Settings.EnabledChange += this.OnEnableChange;
            Settings.DesktopOffsetChange += this.OnDesktopOffsetChange;
            Settings.ModelVisibilityChange += this.OnModelVisibilityChange;
            Settings.TrackingModeChange += this.OnTrackingModeChange;
            Settings.RootAngleChange += this.OnRootAngleChange;
            Settings.HeadAttachChange += this.OnHeadAttachChange;
            Settings.HeadOffsetChange += this.OnHeadOffsetChange;

            m_leapController = new Leap.Controller();
            m_leapController.Device += this.OnLeapDeviceInitialized;
            m_leapController.DeviceFailure += this.OnLeapDeviceFailure;
            m_leapController.DeviceLost += this.OnLeapDeviceLost;
            m_leapController.Connect += this.OnLeapServiceConnect;
            m_leapController.Disconnect += this.OnLeapServiceDisconnect;

            m_gesturesData = new GestureMatcher.GesturesData();
            m_leapHands = new GameObject[GestureMatcher.GesturesData.ms_handsCount];
            m_leapElbows = new GameObject[GestureMatcher.GesturesData.ms_handsCount];

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

            m_isInVR = Utils.IsInVR();

            m_leapTrackingRoot = new GameObject("[LeapRoot]");

            for(int i = 0; i < GestureMatcher.GesturesData.ms_handsCount; i++)
            {
                m_leapHands[i] = new GameObject("LeapHand" + i);
                m_leapHands[i].transform.parent = m_leapTrackingRoot.transform;
                m_leapHands[i].transform.localPosition = Vector3.zero;
                m_leapHands[i].transform.localRotation = Quaternion.identity;

                m_leapElbows[i] = new GameObject("LeapElbow" + i);
                m_leapElbows[i].transform.parent = m_leapTrackingRoot.transform;
                m_leapElbows[i].transform.localPosition = Vector3.zero;
                m_leapElbows[i].transform.localRotation = Quaternion.identity;
            }

            m_leapControllerModel = AssetsHandler.GetAsset("assets/models/leapmotion/leap_motion_1_0.obj");
            if(m_leapControllerModel != null)
            {
                m_leapControllerModel.name = "LeapModel";
                m_leapControllerModel.transform.parent = m_leapTrackingRoot.transform;
                m_leapControllerModel.transform.localPosition = Vector3.zero;
                m_leapControllerModel.transform.localRotation = Quaternion.identity;
            }

            // Player setup
            m_leapTracked = PlayerSetup.Instance.gameObject.AddComponent<LeapTracked>();
            m_leapTracked.SetTransforms(m_leapHands[0].transform, m_leapHands[1].transform, m_leapElbows[0].transform, m_leapElbows[1].transform);
            m_leapTracked.SetEnabled(Settings.Enabled);
            m_leapTracked.SetTrackElbows(Settings.TrackElbows);
            m_leapTracked.SetFingersOnly(Settings.FingersOnly);

            OnEnableChange(Settings.Enabled);
            OnModelVisibilityChange(Settings.ModelVisibility);
            OnTrackingModeChange(Settings.TrackingMode);
            OnHeadAttachChange(Settings.HeadAttach); // Includes offsets and parenting
        }

        public override void OnDeinitializeMelon()
        {
            if(ms_instance == this)
                ms_instance = null;

            m_leapController?.StopConnection();
            m_leapController?.Dispose();
            m_leapController = null;

            m_gesturesData = null;
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
                                Utils.LeapToUnity(ref l_pos, ref l_rot, Settings.TrackingMode);
                                m_leapHands[i].transform.localPosition = l_pos;
                                m_leapHands[i].transform.localRotation = l_rot;

                                l_pos = m_gesturesData.m_elbowsPositions[i];
                                Utils.LeapToUnity(ref l_pos, ref l_rot, Settings.TrackingMode);
                                m_leapElbows[i].transform.localPosition = l_pos;
                            }
                        }
                    }
                }

                if(m_leapTracked != null)
                    m_leapTracked.UpdateTracking(m_gesturesData);
            }
        }

        public override void OnLateUpdate()
        {
            if(Settings.Enabled && !m_isInVR && (m_leapTracked != null))
                m_leapTracked.UpdateTrackingLate(m_gesturesData);
        }

        // Settings changes
        void OnEnableChange(bool p_state)
        {
            if(p_state)
            {
                m_leapController?.StartConnection();
                UpdateDeviceTrackingMode();
            }
            else
                m_leapController?.StopConnection();
        }

        void OnDesktopOffsetChange(Vector3 p_offset)
        {
            if((m_leapTrackingRoot != null) && !Settings.HeadAttach)
            {
                if(!m_isInVR)
                    m_leapTrackingRoot.transform.localPosition = p_offset * PlayerSetup.Instance.vrCameraRig.transform.localScale.x;
                else
                    m_leapTrackingRoot.transform.localPosition = p_offset;
            }
        }

        void OnModelVisibilityChange(bool p_state)
        {
            if(m_leapControllerModel != null)
                m_leapControllerModel.SetActive(p_state);
        }

        void OnTrackingModeChange(Settings.LeapTrackingMode p_mode)
        {
            if(Settings.Enabled)
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

        void OnRootAngleChange(Vector3 p_angle)
        {
            if(m_leapTrackingRoot != null)
                m_leapTrackingRoot.transform.localRotation = Quaternion.Euler(p_angle);
        }

        void OnHeadAttachChange(bool p_state)
        {
            if(m_leapTrackingRoot != null)
            {
                if(p_state)
                {
                    if(!m_isInVR)
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
                    if(!m_isInVR)
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

                m_leapTrackingRoot.transform.localRotation = Quaternion.Euler(Settings.RootAngle);
            }
        }

        void OnHeadOffsetChange(Vector3 p_offset)
        {
            if((m_leapTrackingRoot != null) && Settings.HeadAttach)
            {
                if(!m_isInVR)
                    m_leapTrackingRoot.transform.localPosition = p_offset * PlayerSetup.Instance.vrCameraRig.transform.localScale.x;
                else
                    m_leapTrackingRoot.transform.localPosition = p_offset;
            }
        }

        // Internal utility
        void UpdateDeviceTrackingMode()
        {
            m_leapController?.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP, null);
            m_leapController?.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD, null);

            switch(Settings.TrackingMode)
            {
                case Settings.LeapTrackingMode.Screentop:
                    m_leapController?.SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP, null);
                    break;
                case Settings.LeapTrackingMode.HMD:
                    m_leapController?.SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD, null);
                    break;
            }
        }

        // Leap events
        void OnLeapDeviceInitialized(object p_sender, Leap.DeviceEventArgs p_args)
        {
            if(Settings.Enabled)
            {
                m_leapController?.SubscribeToDeviceEvents(p_args.Device);
                UpdateDeviceTrackingMode();
            }

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
            m_leapController?.UnsubscribeFromDeviceEvents(p_args.Device);

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
        static void OnAvatarClear_Postfix() => ms_instance?.OnAvatarClear();
        void OnAvatarClear()
        {
            try
            {
                if(m_leapTracked != null)
                    m_leapTracked.OnAvatarClear();
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
                if(m_leapTracked != null)
                    m_leapTracked.OnSetupAvatar();

                OnHeadAttachChange(Settings.HeadAttach);
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

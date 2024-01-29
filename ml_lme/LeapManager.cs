using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Systems.InputManagement;
using System.Collections;
using UnityEngine;

namespace ml_lme
{
    [DisallowMultipleComponent]
    class LeapManager : MonoBehaviour
    {
        public static LeapManager Instance { get; private set; } = null;

        Leap.Controller m_leapController = null;
        LeapParser.LeapData m_leapData = null;

        LeapTracking m_leapTracking = null;
        LeapTracked m_leapTracked = null;
        LeapInput m_leapInput = null;

        void Awake()
        {
            if(Instance == null)
                Instance = this;

            m_leapController = new Leap.Controller();
            m_leapData = new LeapParser.LeapData();

            DontDestroyOnLoad(this);

            m_leapController.Device += this.OnLeapDeviceInitialized;
            m_leapController.DeviceFailure += this.OnLeapDeviceFailure;
            m_leapController.DeviceLost += this.OnLeapDeviceLost;
            m_leapController.Connect += this.OnLeapServiceConnect;
            m_leapController.Disconnect += this.OnLeapServiceDisconnect;

            Settings.EnabledChange += this.OnEnableChange;
            Settings.TrackingModeChange += this.OnTrackingModeChange;

            m_leapTracking = new GameObject("[LeapTrackingRoot]").AddComponent<LeapTracking>();
            m_leapTracking.transform.parent = this.transform;

            OnEnableChange(Settings.Enabled);
            OnTrackingModeChange(Settings.TrackingMode);

            MelonLoader.MelonCoroutines.Start(WaitForObjects());
        }

        void OnDestroy()
        {
            if(Instance == this)
                Instance = null;

            m_leapController.StopConnection();
            m_leapController.Device -= this.OnLeapDeviceInitialized;
            m_leapController.DeviceFailure -= this.OnLeapDeviceFailure;
            m_leapController.DeviceLost -= this.OnLeapDeviceLost;
            m_leapController.Connect -= this.OnLeapServiceConnect;
            m_leapController.Disconnect -= this.OnLeapServiceDisconnect;
            m_leapController.Dispose();
            m_leapController = null;

            if(m_leapTracking != null)
                Object.Destroy(m_leapTracking);
            m_leapTracking = null;

            if(m_leapTracked != null)
                Object.Destroy(m_leapTracked);
            m_leapTracked = null;

            if(m_leapInput != null)
            {
                if(CVRInputManager.Instance != null)
                    CVRInputManager.Instance.DestroyInputModule(m_leapInput);
                else
                    m_leapInput.ModuleDestroyed();
            }
            m_leapInput = null;

            Settings.EnabledChange -= this.OnEnableChange;
            Settings.TrackingModeChange -= this.OnTrackingModeChange;
        }

        IEnumerator WaitForObjects()
        {
            while(CVRInputManager.Instance == null)
                yield return null;

            while(PlayerSetup.Instance == null)
                yield return null;

            while(LeapTracking.Instance == null)
                yield return null;

            m_leapInput = new LeapInput();
            CVRInputManager.Instance.AddInputModule(m_leapInput);

            m_leapTracked = PlayerSetup.Instance.gameObject.AddComponent<LeapTracked>();
        }

        void Update()
        {
            if(Settings.Enabled)
            {
                m_leapData.Reset();

                if(m_leapController.IsConnected)
                {
                    Leap.Frame l_frame = m_leapController.Frame();
                    LeapParser.ParseFrame(l_frame, m_leapData);
                }
            }
        }

        public LeapParser.LeapData GetLatestData() => m_leapData;

        // Device events
        void OnLeapDeviceInitialized(object p_sender, Leap.DeviceEventArgs p_args)
        {
            if(Settings.Enabled)
            {
                m_leapController.SubscribeToDeviceEvents(p_args.Device);
                UpdateDeviceTrackingMode();
            }

            Utils.ShowHUDNotification("Leap Motion Extension", "Device initialized");
        }

        void OnLeapDeviceFailure(object p_sender, Leap.DeviceFailureEventArgs p_args)
        {
            Utils.ShowHUDNotification("Leap Motion Extension", "Device failure", "Code " + p_args.ErrorCode + ": " + p_args.ErrorMessage);
        }

        void OnLeapDeviceLost(object p_sender, Leap.DeviceEventArgs p_args)
        {
            m_leapController.UnsubscribeFromDeviceEvents(p_args.Device);

            Utils.ShowHUDNotification("Leap Motion Extension", "Device lost");
        }

        void OnLeapServiceConnect(object p_sender, Leap.ConnectionEventArgs p_args)
        {
            Utils.ShowHUDNotification("Leap Motion Extension", "Service connected");
        }

        void OnLeapServiceDisconnect(object p_sender, Leap.ConnectionLostEventArgs p_args)
        {
            Utils.ShowHUDNotification("Leap Motion Extension", "Service disconnected");
        }

        // Settings
        void OnEnableChange(bool p_state)
        {
            if(p_state)
            {
                m_leapController.StartConnection();
                UpdateDeviceTrackingMode();
            }
            else
                m_leapController.StopConnection();
        }

        void OnTrackingModeChange(Settings.LeapTrackingMode p_mode)
        {
            if(Settings.Enabled)
                UpdateDeviceTrackingMode();
        }

        // Game events
        internal void OnAvatarClear()
        {
            if(m_leapTracking != null)
                m_leapTracking.OnAvatarClear();
            if(m_leapTracked != null)
                m_leapTracked.OnAvatarClear();
        }

        internal void OnAvatarSetup()
        {
            if(m_leapTracking != null)
                m_leapTracking.OnAvatarSetup();

            if(m_leapTracked != null)
                m_leapTracked.OnAvatarSetup();
        }

        internal void OnAvatarReinitialize()
        {
            if(m_leapTracked != null)
                m_leapTracked.OnAvatarReinitialize();
        }

        internal void OnRayScale(float p_scale)
        {
            m_leapInput?.OnRayScale(p_scale);
        }

        internal void OnPlayspaceScale(float p_relation)
        {
            if(m_leapTracking != null)
                m_leapTracking.OnPlayspaceScale(p_relation);
        }

        internal void OnPickupGrab(CVRPickupObject p_pickup)
        {
            m_leapInput?.OnPickupGrab(p_pickup);
        }

        // Arbitrary
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
    }
}

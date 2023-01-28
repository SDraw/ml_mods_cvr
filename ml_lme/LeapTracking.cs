using ABI_RC.Core.Player;
using System.Collections;
using UnityEngine;

namespace ml_lme
{
    [DisallowMultipleComponent]
    class LeapTracking : MonoBehaviour
    {
        static LeapTracking ms_instance = null;
        static Quaternion ms_identityRotation = Quaternion.identity;

        bool m_inVR = false;

        GameObject m_leapHandLeft = null;
        GameObject m_leapHandRight = null;
        GameObject m_leapElbowLeft = null;
        GameObject m_leapElbowRight = null;
        GameObject m_leapControllerModel = null;

        public static LeapTracking GetInstance() => ms_instance;

        void Start()
        {
            if(ms_instance == null)
                ms_instance = this;

            m_inVR = Utils.IsInVR();

            m_leapHandLeft = new GameObject("LeapHandLeft");
            m_leapHandLeft.transform.parent = this.transform;
            m_leapHandLeft.transform.localPosition = Vector3.zero;
            m_leapHandLeft.transform.localRotation = Quaternion.identity;

            m_leapHandRight = new GameObject("LeapHandRight");
            m_leapHandRight.transform.parent = this.transform;
            m_leapHandRight.transform.localPosition = Vector3.zero;
            m_leapHandRight.transform.localRotation = Quaternion.identity;

            m_leapElbowLeft = new GameObject("LeapElbowLeft");
            m_leapElbowLeft.transform.parent = this.transform;
            m_leapElbowLeft.transform.localPosition = Vector3.zero;
            m_leapElbowLeft.transform.localRotation = Quaternion.identity;

            m_leapElbowRight = new GameObject("LeapElbowRight");
            m_leapElbowRight.transform.parent = this.transform;
            m_leapElbowRight.transform.localPosition = Vector3.zero;
            m_leapElbowRight.transform.localRotation = Quaternion.identity;

            m_leapControllerModel = AssetsHandler.GetAsset("assets/models/leapmotion/leap_motion_1_0.obj");
            if(m_leapControllerModel != null)
            {
                m_leapControllerModel.name = "LeapModel";
                m_leapControllerModel.transform.parent = this.transform;
                m_leapControllerModel.transform.localPosition = Vector3.zero;
                m_leapControllerModel.transform.localRotation = Quaternion.identity;
            }

            Settings.DesktopOffsetChange += this.OnDesktopOffsetChange;
            Settings.ModelVisibilityChange += this.OnModelVisibilityChange;
            Settings.TrackingModeChange += this.OnTrackingModeChange;
            Settings.RootAngleChange += this.OnRootAngleChange;
            Settings.HeadAttachChange += this.OnHeadAttachChange;
            Settings.HeadOffsetChange += this.OnHeadOffsetChange;

            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());

            OnModelVisibilityChange(Settings.ModelVisibility);
            OnTrackingModeChange(Settings.TrackingMode);
            OnRootAngleChange(Settings.RootAngle);
        }

        IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            OnDesktopOffsetChange(Settings.DesktopOffset);
            OnHeadAttachChange(Settings.HeadAttach);
            OnHeadOffsetChange(Settings.HeadOffset);
        }

        void OnDestroy()
        {
            if(ms_instance == this)
                ms_instance = null;

            Settings.DesktopOffsetChange -= this.OnDesktopOffsetChange;
            Settings.ModelVisibilityChange -= this.OnModelVisibilityChange;
            Settings.TrackingModeChange -= this.OnTrackingModeChange;
            Settings.RootAngleChange -= this.OnRootAngleChange;
            Settings.HeadAttachChange -= this.OnHeadAttachChange;
            Settings.HeadOffsetChange -= this.OnHeadOffsetChange;
        }

        void Update()
        {
            if(Settings.Enabled)
            {
                GestureMatcher.LeapData l_data = LeapManager.GetInstance().GetLatestData();

                if(l_data.m_leftHand.m_present)
                {
                    Utils.LeapToUnity(ref l_data.m_leftHand.m_position, ref l_data.m_leftHand.m_rotation, Settings.TrackingMode);
                    m_leapHandLeft.transform.localPosition = l_data.m_leftHand.m_position;
                    m_leapHandLeft.transform.localRotation = l_data.m_leftHand.m_rotation;

                    Utils.LeapToUnity(ref l_data.m_leftHand.m_elbowPosition, ref ms_identityRotation, Settings.TrackingMode);
                    m_leapElbowLeft.transform.localPosition = l_data.m_leftHand.m_elbowPosition;
                }

                if(l_data.m_rightHand.m_present)
                {
                    Utils.LeapToUnity(ref l_data.m_rightHand.m_position, ref l_data.m_rightHand.m_rotation, Settings.TrackingMode);
                    m_leapHandRight.transform.localPosition = l_data.m_rightHand.m_position;
                    m_leapHandRight.transform.localRotation = l_data.m_rightHand.m_rotation;

                    Utils.LeapToUnity(ref l_data.m_rightHand.m_elbowPosition, ref ms_identityRotation, Settings.TrackingMode);
                    m_leapElbowRight.transform.localPosition = l_data.m_rightHand.m_elbowPosition;
                }
            }
        }

        public Transform GetLeftHand() => m_leapHandLeft.transform;
        public Transform GetRightHand() => m_leapHandRight.transform;
        public Transform GetLeftElbow() => m_leapElbowLeft.transform;
        public Transform GetRightElbow() => m_leapElbowRight.transform;

        // Settings
        void OnDesktopOffsetChange(Vector3 p_offset)
        {
            if(!Settings.HeadAttach)
            {
                if(!m_inVR)
                    this.transform.localPosition = p_offset * PlayerSetup.Instance.vrCameraRig.transform.localScale.x;
                else
                    this.transform.localPosition = p_offset;
            }
        }

        void OnModelVisibilityChange(bool p_state)
        {
            m_leapControllerModel.SetActive(p_state);
        }

        void OnTrackingModeChange(Settings.LeapTrackingMode p_mode)
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

        void OnRootAngleChange(Vector3 p_angle)
        {
            this.transform.localRotation = Quaternion.Euler(p_angle);
        }

        void OnHeadAttachChange(bool p_state)
        {
            if(p_state)
            {
                if(!m_inVR)
                {
                    this.transform.parent = PlayerSetup.Instance.desktopCamera.transform;
                    this.transform.localPosition = Settings.HeadOffset * PlayerSetup.Instance.vrCameraRig.transform.localScale.x;
                    this.transform.localScale = PlayerSetup.Instance.vrCameraRig.transform.localScale;
                }
                else
                {
                    this.transform.parent = PlayerSetup.Instance.vrCamera.transform;
                    this.transform.localPosition = Settings.HeadOffset;
                    this.transform.localScale = Vector3.one;
                }
            }
            else
            {
                if(!m_inVR)
                {
                    this.transform.parent = PlayerSetup.Instance.desktopCameraRig.transform;
                    this.transform.localPosition = Settings.DesktopOffset * PlayerSetup.Instance.vrCameraRig.transform.localScale.x;
                    this.transform.localScale = PlayerSetup.Instance.vrCameraRig.transform.localScale;
                }
                else
                {
                    this.transform.parent = PlayerSetup.Instance.vrCameraRig.transform;
                    this.transform.localPosition = Settings.DesktopOffset;
                    this.transform.localScale = Vector3.one;
                }
            }

            this.transform.localRotation = Quaternion.Euler(Settings.RootAngle);
        }

        void OnHeadOffsetChange(Vector3 p_offset)
        {
            if(Settings.HeadAttach)
            {
                if(!m_inVR)
                    this.transform.localPosition = p_offset * PlayerSetup.Instance.vrCameraRig.transform.localScale.x;
                else
                    this.transform.localPosition = p_offset;
            }
        }

        // Game events
        internal void OnAvatarSetup()
        {
            m_inVR = Utils.IsInVR();
            OnHeadAttachChange(Settings.HeadAttach);
        }
    }
}

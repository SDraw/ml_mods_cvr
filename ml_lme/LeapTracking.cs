using ABI_RC.Core.Player;
using ABI_RC.Systems.VRModeSwitch;
using System.Collections;
using UnityEngine;

namespace ml_lme
{
    [DisallowMultipleComponent]
    class LeapTracking : MonoBehaviour
    {
        public static LeapTracking Instance { get; private set; } = null;
        static Quaternion ms_dummyRotation = Quaternion.identity;
        static readonly Quaternion ms_hmdRotation = new Quaternion(0f, 0.7071068f, 0.7071068f, 0f);
        static readonly Quaternion ms_screentopRotation = new Quaternion(0f, 0f, -1f, 0f);

        bool m_inVR = false;

        GameObject m_leapHands = null;
        LeapHand m_leapHandLeft = null;
        LeapHand m_leapHandRight = null;
        Transform m_leapElbowLeft = null;
        Transform m_leapElbowRight = null;
        GameObject m_leapControllerModel = null;

        float m_scaleRelation = 1f;

        void Start()
        {
            if(Instance == null)
                Instance = this;

            m_inVR = Utils.IsInVR();

            m_leapElbowLeft = new GameObject("LeapElbowLeft").transform;
            m_leapElbowLeft.parent = this.transform;
            m_leapElbowLeft.localPosition = Vector3.zero;
            m_leapElbowLeft.localRotation = Quaternion.identity;

            m_leapElbowRight = new GameObject("LeapElbowRight").transform;
            m_leapElbowRight.parent = this.transform;
            m_leapElbowRight.localPosition = Vector3.zero;
            m_leapElbowRight.localRotation = Quaternion.identity;

            m_leapControllerModel = AssetsHandler.GetAsset("assets/models/leapmotion/leap_motion_1_0.obj");
            if(m_leapControllerModel != null)
            {
                m_leapControllerModel.name = "LeapModel";
                m_leapControllerModel.transform.parent = this.transform;
                m_leapControllerModel.transform.localPosition = Vector3.zero;
                m_leapControllerModel.transform.localRotation = Quaternion.identity;
            }

            m_leapHands = AssetsHandler.GetAsset("assets/models/leaphands/leaphands.prefab");
            if(m_leapHands != null)
            {
                m_leapHands.name = "LeapHands";
                m_leapHands.transform.parent = this.transform;
                m_leapHands.transform.localPosition = Vector3.zero;
                m_leapHands.transform.localRotation = Quaternion.identity;

                m_leapHandLeft = new LeapHand(m_leapHands.transform.Find("HandL"), true);
                m_leapHandRight = new LeapHand(m_leapHands.transform.Find("HandR"), false);
            }

            Settings.DesktopOffsetChange += this.OnDesktopOffsetChange;
            Settings.ModelVisibilityChange += this.OnModelVisibilityChange;
            Settings.VisualHandsChange += this.OnVisualHandsChange;
            Settings.TrackingModeChange += this.OnTrackingModeChange;
            Settings.RootAngleChange += this.OnRootAngleChange;
            Settings.HeadAttachChange += this.OnHeadAttachChange;
            Settings.HeadOffsetChange += this.OnHeadOffsetChange;

            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());

            OnModelVisibilityChange(Settings.ModelVisibility);
            OnVisualHandsChange(Settings.VisualHands);
            OnTrackingModeChange(Settings.TrackingMode);
            OnRootAngleChange(Settings.RootAngle);

            VRModeSwitchEvents.OnInitializeXR.AddListener(this.OnModeSwitch);
            VRModeSwitchEvents.OnDeinitializeXR.AddListener(this.OnModeSwitch);
        }

        IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            OnHeadAttachChange(Settings.HeadAttach);
        }

        void OnDestroy()
        {
            if(Instance == this)
                Instance = null;

            if(m_leapHands != null)
                Object.Destroy(m_leapHands);
            m_leapHands = null;
            m_leapHandLeft = null;
            m_leapHandRight = null;

            if(m_leapElbowLeft != null)
                Object.Destroy(m_leapElbowLeft.gameObject);
            m_leapElbowLeft = null;

            if(m_leapElbowRight != null)
                Object.Destroy(m_leapElbowRight.gameObject);
            m_leapElbowRight = null;

            if(m_leapControllerModel != null)
                Object.Destroy(m_leapControllerModel);
            m_leapControllerModel = null;

            Settings.DesktopOffsetChange -= this.OnDesktopOffsetChange;
            Settings.ModelVisibilityChange -= this.OnModelVisibilityChange;
            Settings.VisualHandsChange -= this.OnVisualHandsChange;
            Settings.TrackingModeChange -= this.OnTrackingModeChange;
            Settings.RootAngleChange -= this.OnRootAngleChange;
            Settings.HeadAttachChange -= this.OnHeadAttachChange;
            Settings.HeadOffsetChange -= this.OnHeadOffsetChange;

            VRModeSwitchEvents.OnInitializeXR.RemoveListener(this.OnModeSwitch);
            VRModeSwitchEvents.OnDeinitializeXR.RemoveListener(this.OnModeSwitch);
        }

        void Update()
        {
            if(Settings.Enabled)
            {
                LeapParser.LeapData l_data = LeapManager.Instance.GetLatestData();

                if(l_data.m_leftHand.m_present)
                {
                    OrientationAdjustment(ref l_data.m_leftHand.m_position, ref l_data.m_leftHand.m_rotation, Settings.TrackingMode);
                    for(int i = 0; i < 20; i++)
                        OrientationAdjustment(ref l_data.m_leftHand.m_fingerPosition[i], ref l_data.m_leftHand.m_fingerRotation[i], Settings.TrackingMode);

                    m_leapHandLeft.GetRoot().localPosition = l_data.m_leftHand.m_position;
                    m_leapHandLeft.GetRoot().localRotation = l_data.m_leftHand.m_rotation;

                    OrientationAdjustment(ref l_data.m_leftHand.m_elbowPosition, ref ms_dummyRotation, Settings.TrackingMode);
                    m_leapElbowLeft.localPosition = l_data.m_leftHand.m_elbowPosition;

                    m_leapHandLeft?.Update(l_data.m_leftHand);
                }

                if(l_data.m_rightHand.m_present)
                {
                    OrientationAdjustment(ref l_data.m_rightHand.m_position, ref l_data.m_rightHand.m_rotation, Settings.TrackingMode);
                    for(int i = 0; i < 20; i++)
                        OrientationAdjustment(ref l_data.m_rightHand.m_fingerPosition[i], ref l_data.m_rightHand.m_fingerRotation[i], Settings.TrackingMode);

                    m_leapHandRight.GetRoot().localPosition = l_data.m_rightHand.m_position;
                    m_leapHandRight.GetRoot().localRotation = l_data.m_rightHand.m_rotation;

                    OrientationAdjustment(ref l_data.m_rightHand.m_elbowPosition, ref ms_dummyRotation, Settings.TrackingMode);
                    m_leapElbowRight.localPosition = l_data.m_rightHand.m_elbowPosition;

                    m_leapHandRight?.Update(l_data.m_rightHand);
                }
            }
        }

        public LeapHand GetLeftHand() => m_leapHandLeft;
        public LeapHand GetRightHand() => m_leapHandRight;
        public Transform GetLeftElbow() => m_leapElbowLeft;
        public Transform GetRightElbow() => m_leapElbowRight;
        public void Rebind(Quaternion p_base)
        {
            m_leapHandLeft?.Rebind(p_base);
            m_leapHandRight?.Rebind(p_base);
        }

        // Settings
        void OnDesktopOffsetChange(Vector3 p_offset)
        {
            if(!Settings.HeadAttach)
                this.transform.localPosition = p_offset * (!m_inVR ? m_scaleRelation : 1f);
        }

        void OnModelVisibilityChange(bool p_state)
        {
            m_leapControllerModel.SetActive(p_state);
        }

        void OnVisualHandsChange(bool p_state)
        {
            m_leapHandLeft?.SetMeshActive(p_state);
            m_leapHandRight?.SetMeshActive(p_state);
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
            if(!m_inVR)
            {
                this.transform.parent = (p_state ? PlayerSetup.Instance.desktopCamera.transform : PlayerSetup.Instance.desktopCameraRig.transform);
                this.transform.localPosition = (p_state ? Settings.HeadOffset : Settings.DesktopOffset) * m_scaleRelation;
            }
            else
            {
                this.transform.parent = (p_state ? PlayerSetup.Instance.vrCamera.transform : PlayerSetup.Instance.vrCameraRig.transform);
                this.transform.localPosition = (p_state ? Settings.HeadOffset : Settings.DesktopOffset);
            }

            this.transform.localScale = Vector3.one * (!m_inVR ? m_scaleRelation : 1f);
            this.transform.localRotation = Quaternion.Euler(Settings.RootAngle);
        }

        void OnHeadOffsetChange(Vector3 p_offset)
        {
            if(Settings.HeadAttach)
                this.transform.localPosition = p_offset * (!m_inVR ? m_scaleRelation : 1f);
        }

        // Game events
        internal void OnAvatarClear()
        {
            m_scaleRelation = 1f;
            OnHeadAttachChange(Settings.HeadAttach);
        }

        internal void OnAvatarSetup()
        {
            m_inVR = Utils.IsInVR();
            OnHeadAttachChange(Settings.HeadAttach);
        }

        internal void OnPlayspaceScale(float p_relation)
        {
            m_scaleRelation = p_relation;
            OnHeadAttachChange(Settings.HeadAttach);
        }

        void OnModeSwitch()
        {
            m_inVR = Utils.IsInVR();
            OnHeadAttachChange(Settings.HeadAttach);
        }

        // Utils
        static void OrientationAdjustment(ref Vector3 p_pos, ref Quaternion p_rot, Settings.LeapTrackingMode p_mode)
        {
            switch(p_mode)
            {
                case Settings.LeapTrackingMode.Screentop:
                {
                    p_pos.x *= -1f;
                    p_pos.y *= -1f;
                    p_rot = (ms_screentopRotation * p_rot);
                }
                break;

                case Settings.LeapTrackingMode.HMD:
                {
                    p_pos.x *= -1f;
                    Utils.Swap(ref p_pos.y, ref p_pos.z);
                    p_rot = (ms_hmdRotation * p_rot);
                }
                break;
            }
        }
    }
}

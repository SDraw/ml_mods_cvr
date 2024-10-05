using ABI_RC.Core.Player;
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

        Transform m_root = null;
        Transform m_offsetPoint = null;
        GameObject m_leapHands = null;
        LeapHand m_leapHandLeft = null;
        LeapHand m_leapHandRight = null;
        Transform m_leapElbowLeft = null;
        Transform m_leapElbowRight = null;
        GameObject m_leapControllerModel = null;

        void Start()
        {
            if(Instance != null)
            {
                Object.DestroyImmediate(this);
                return;
            }

            Instance = this;

            m_root = new GameObject("Root").transform;
            m_root.parent = this.transform;
            m_root.localPosition = Vector3.zero;
            m_root.localRotation = Quaternion.identity;

            m_offsetPoint = new GameObject("OffsetPoint").transform;
            m_offsetPoint.parent = m_root;
            m_offsetPoint.localPosition = Vector3.zero;
            m_offsetPoint.localRotation = Quaternion.identity;

            m_leapElbowLeft = new GameObject("LeapElbowLeft").transform;
            m_leapElbowLeft.parent = m_offsetPoint;
            m_leapElbowLeft.localPosition = Vector3.zero;
            m_leapElbowLeft.localRotation = Quaternion.identity;

            m_leapElbowRight = new GameObject("LeapElbowRight").transform;
            m_leapElbowRight.parent = m_offsetPoint;
            m_leapElbowRight.localPosition = Vector3.zero;
            m_leapElbowRight.localRotation = Quaternion.identity;

            m_leapControllerModel = AssetsHandler.GetAsset("assets/models/leapmotion/leap_motion_1_0.obj");
            if(m_leapControllerModel != null)
            {
                m_leapControllerModel.name = "LeapModel";
                m_leapControllerModel.transform.parent = m_offsetPoint;
                m_leapControllerModel.transform.localPosition = Vector3.zero;
                m_leapControllerModel.transform.localRotation = Quaternion.identity;
            }

            m_leapHands = AssetsHandler.GetAsset("assets/models/leaphands/leaphands.prefab");
            if(m_leapHands != null)
            {
                m_leapHands.name = "LeapHands";
                m_leapHands.transform.parent = m_offsetPoint;
                m_leapHands.transform.localPosition = Vector3.zero;
                m_leapHands.transform.localRotation = Quaternion.identity;

                m_leapHandLeft = new LeapHand(m_leapHands.transform.Find("HandL"), true);
                m_leapHandRight = new LeapHand(m_leapHands.transform.Find("HandR"), false);
            }

            OnModelVisibilityChanged(Settings.ModelVisibility);
            OnVisualHandsChanged(Settings.VisualHands);
            OnTrackingModeChanged(Settings.TrackingMode);
            OnHeadAttachChanged(Settings.HeadAttach);
            OnRootAngleChanged(Settings.RootAngle);

            Settings.OnEnabledChanged.AddListener(this.OnEnabledChanged);
            Settings.OnModelVisibilityChanged.AddListener(this.OnModelVisibilityChanged);
            Settings.OnVisualHandsChanged.AddListener(this.OnVisualHandsChanged);
            Settings.OnTrackingModeChanged.AddListener(this.OnTrackingModeChanged);
            Settings.OnHeadAttachChanged.AddListener(this.OnHeadAttachChanged);
            Settings.OnHeadOffsetChanged.AddListener(this.OnHeadOffsetChanged);
            Settings.OnDesktopOffsetChanged.AddListener(this.OnDesktopOffsetChanged);
            Settings.OnRootAngleChanged.AddListener(this.OnRootAngleChanged);

            GameEvents.OnPlayspaceScale.AddListener(this.OnPlayspaceScale);
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

            if(m_offsetPoint != null)
                Destroy(m_offsetPoint.gameObject);
            m_offsetPoint = null;

            if(m_root != null)
                Destroy(m_root.gameObject);
            m_root = null;

            Settings.OnEnabledChanged.RemoveListener(this.OnEnabledChanged);
            Settings.OnModelVisibilityChanged.RemoveListener(this.OnModelVisibilityChanged);
            Settings.OnVisualHandsChanged.RemoveListener(this.OnVisualHandsChanged);
            Settings.OnTrackingModeChanged.RemoveListener(this.OnTrackingModeChanged);
            Settings.OnHeadAttachChanged.RemoveListener(this.OnHeadAttachChanged);
            Settings.OnHeadOffsetChanged.RemoveListener(this.OnHeadOffsetChanged);
            Settings.OnDesktopOffsetChanged.RemoveListener(this.OnDesktopOffsetChanged);
            Settings.OnRootAngleChanged.RemoveListener(this.OnRootAngleChanged);

            GameEvents.OnPlayspaceScale.RemoveListener(this.OnPlayspaceScale);
        }

        void Update()
        {
            if(Settings.Enabled)
            {
                Transform l_camera = PlayerSetup.Instance.GetActiveCamera().transform;
                m_root.position = l_camera.position;
                m_root.rotation = (Settings.HeadAttach ? l_camera.rotation : PlayerSetup.Instance.GetPlayerRotation());

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

                    m_leapHandLeft.Update(l_data.m_leftHand);
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

                    m_leapHandRight.Update(l_data.m_rightHand);
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
        void OnEnabledChanged(bool p_state)
        {
            OnModelVisibilityChanged(Settings.ModelVisibility);
            OnVisualHandsChanged(Settings.VisualHands);
        }

        void OnModelVisibilityChanged(bool p_state)
        {
            if(m_leapControllerModel != null)
                m_leapControllerModel.SetActive(Settings.Enabled && p_state);
        }

        void OnVisualHandsChanged(bool p_state)
        {
            m_leapHandLeft?.SetMeshActive(Settings.Enabled && p_state);
            m_leapHandRight?.SetMeshActive(Settings.Enabled && p_state);
        }

        void OnTrackingModeChanged(Settings.LeapTrackingMode p_mode)
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

        void OnHeadAttachChanged(bool p_state)
        {
            if(m_offsetPoint != null)
                m_offsetPoint.localPosition = (p_state ? Settings.HeadOffset : Settings.DesktopOffset);
        }

        void OnHeadOffsetChanged(Vector3 p_offset)
        {
            if(Settings.HeadAttach && (m_offsetPoint != null))
                m_offsetPoint.localPosition = p_offset;
        }

        void OnDesktopOffsetChanged(Vector3 p_offset)
        {
            if(!Settings.HeadAttach && (m_offsetPoint != null))
                m_offsetPoint.localPosition = p_offset;
        }

        void OnRootAngleChanged(Vector3 p_angle)
        {
            if(m_offsetPoint != null)
                m_offsetPoint.localRotation = Quaternion.Euler(p_angle);
        }

        // Game events
        void OnPlayspaceScale(float p_relation)
        {
            try
            {
                if(m_root != null)
                    m_root.localScale = Vector3.one * p_relation;
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
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

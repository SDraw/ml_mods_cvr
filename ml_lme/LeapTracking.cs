using ABI_RC.Core.Player;
using System.Collections;
using UnityEngine;

namespace ml_lme
{
    [DisallowMultipleComponent]
    class LeapTracking : MonoBehaviour
    {
        static LeapTracking ms_instance = null;
        static Quaternion ms_dummyRotation = Quaternion.identity;
        static readonly Quaternion ms_hmdRotation = new Quaternion(0f, 0.7071068f, 0.7071068f, 0f);
        static readonly Quaternion ms_screentopRotation = new Quaternion(0f, 0f, -1f, 0f);

        bool m_inVR = false;

        GameObject m_leapHandLeft = null;
        GameObject m_leapHandRight = null;
        GameObject m_leapElbowLeft = null;
        GameObject m_leapElbowRight = null;
        GameObject m_leapControllerModel = null;
        GameObject m_visualHands = null;
        VisualHand m_visualHandLeft = null;
        VisualHand m_visualHandRight = null;

        float m_scaleRelation = 1f;

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

            m_visualHands = AssetsHandler.GetAsset("assets/models/hands/leaphands.prefab");
            if(m_visualHands != null)
            {
                m_visualHands.name = "VisualHands";
                m_visualHands.transform.parent = this.transform;
                m_visualHands.transform.localPosition = Vector3.zero;
                m_visualHands.transform.localRotation = Quaternion.identity;

                m_visualHandLeft = new VisualHand(m_visualHands.transform.Find("HandL"), true);
                m_visualHandRight = new VisualHand(m_visualHands.transform.Find("HandR"), false);
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
        }

        IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            OnHeadAttachChange(Settings.HeadAttach);
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
                    OrientationAdjustment(ref l_data.m_leftHand.m_position, ref l_data.m_leftHand.m_rotation, Settings.TrackingMode);
                    for(int i = 0; i < 20; i++)
                        OrientationAdjustment(ref l_data.m_leftHand.m_fingerPosition[i], ref l_data.m_leftHand.m_fingerRotation[i], Settings.TrackingMode);

                    m_leapHandLeft.transform.localPosition = l_data.m_leftHand.m_position;
                    m_leapHandLeft.transform.localRotation = l_data.m_leftHand.m_rotation;

                    OrientationAdjustment(ref l_data.m_leftHand.m_elbowPosition, ref ms_dummyRotation, Settings.TrackingMode);
                    m_leapElbowLeft.transform.localPosition = l_data.m_leftHand.m_elbowPosition;

                    if(Settings.VisualHands)
                        m_visualHandLeft?.Update(l_data.m_leftHand);
                }

                if(l_data.m_rightHand.m_present)
                {
                    OrientationAdjustment(ref l_data.m_rightHand.m_position, ref l_data.m_rightHand.m_rotation, Settings.TrackingMode);
                    for(int i = 0; i < 20; i++)
                        OrientationAdjustment(ref l_data.m_rightHand.m_fingerPosition[i], ref l_data.m_rightHand.m_fingerRotation[i], Settings.TrackingMode);

                    m_leapHandRight.transform.localPosition = l_data.m_rightHand.m_position;
                    m_leapHandRight.transform.localRotation = l_data.m_rightHand.m_rotation;

                    OrientationAdjustment(ref l_data.m_rightHand.m_elbowPosition, ref ms_dummyRotation, Settings.TrackingMode);
                    m_leapElbowRight.transform.localPosition = l_data.m_rightHand.m_elbowPosition;

                    if(Settings.VisualHands)
                        m_visualHandRight?.Update(l_data.m_rightHand);
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
                this.transform.localPosition = p_offset * (!m_inVR ? m_scaleRelation : 1f);
        }

        void OnModelVisibilityChange(bool p_state)
        {
            m_leapControllerModel.SetActive(p_state);
        }

        void OnVisualHandsChange(bool p_state)
        {
            m_visualHands.SetActive(p_state);
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

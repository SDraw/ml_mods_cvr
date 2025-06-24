using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Core.Player.EyeMovement;
using ABI_RC.Systems.FaceTracking;
using ABI_RC.Systems.GameEventSystem;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.VRModeSwitch;
using RootMotion.FinalIK;
using System;
using UnityEngine;
using ViveSR.anipal.Lip;

namespace ml_dht
{
    [DisallowMultipleComponent]
    class HeadTracked : MonoBehaviour
    {
        static HeadTracked ms_instance = null;

        CVRAvatar m_avatarDescriptor = null;
        Transform m_camera = null;
        LookAtIK m_lookIK = null;
        Transform m_headBone = null;

        Vector3 m_headPosition;
        Quaternion m_headRotation;
        Vector2 m_gazeDirection;
        float m_blinkProgress = 0f;
        LipData_v2 m_lipData;
        bool m_lipDataSent = false;

        Quaternion m_bindRotation;
        Quaternion m_lastHeadRotation;

        DataParser m_dataParser = null;
        float m_smoothing = 0.5f;

        internal HeadTracked()
        {
            m_lipData = new LipData_v2();
            m_lipData.frame = 0;
            m_lipData.time = 0;
            m_lipData.image = IntPtr.Zero;
            m_lipData.prediction_data = new PredictionData_v2();
            m_lipData.prediction_data.blend_shape_weight = new float[(int)LipShape_v2.Max];
        }

        // Unity events
        void Awake()
        {
            if(ms_instance != null)
            {
                DestroyImmediate(this);
                return;
            }

            ms_instance = this;
            DontDestroyOnLoad(this);

            m_dataParser = new DataParser();
        }

        void Start()
        {
            OnSmoothingChanged(Settings.Smoothing);

            OnVRModeSwitch(true);

            Settings.OnEnabledChanged.AddListener(this.OnEnabledOrHeadTrackingChanged);
            Settings.OnHeadTrackingChanged.AddListener(this.OnEnabledOrHeadTrackingChanged);
            Settings.OnSmoothingChanged.AddListener(this.OnSmoothingChanged);

            CVRGameEventSystem.Avatar.OnLocalAvatarLoad.AddListener(this.OnAvatarClear);
            CVRGameEventSystem.Avatar.OnLocalAvatarClear.AddListener(this.OnAvatarSetup);
            GameEvents.OnAvatarReuse.AddListener(this.OnAvatarReuse);
            GameEvents.OnEyeControllerUpdate.AddListener(this.OnEyeControllerUpdate);
            GameEvents.OnFaceTrackingUpdate.AddListener(this.UpdateFaceTracking);

            VRModeSwitchEvents.OnCompletedVRModeSwitch.AddListener(this.OnVRModeSwitch);
        }

        void OnDestroy()
        {
            if(ms_instance == this)
                ms_instance = null;

            m_dataParser = null;

            Settings.OnEnabledChanged.RemoveListener(this.OnEnabledOrHeadTrackingChanged);
            Settings.OnHeadTrackingChanged.RemoveListener(this.OnEnabledOrHeadTrackingChanged);
            Settings.OnSmoothingChanged.RemoveListener(this.OnSmoothingChanged);

            CVRGameEventSystem.Avatar.OnLocalAvatarLoad.RemoveListener(this.OnAvatarClear);
            CVRGameEventSystem.Avatar.OnLocalAvatarClear.RemoveListener(this.OnAvatarSetup);
            GameEvents.OnAvatarReuse.RemoveListener(this.OnAvatarReuse);
            GameEvents.OnEyeControllerUpdate.RemoveListener(this.OnEyeControllerUpdate);
            GameEvents.OnFaceTrackingUpdate.RemoveListener(this.UpdateFaceTracking);

            VRModeSwitchEvents.OnCompletedVRModeSwitch.RemoveListener(this.OnVRModeSwitch);
        }

        void Update()
        {
            if(m_lipDataSent)
                m_lipDataSent = false;

            if(Settings.Enabled && (m_dataParser != null))
            {
                m_dataParser.Update();
                UpdateTrackingData(ref m_dataParser.GetLatestTrackingData());
            }
        }

        // Tracking updates
        public void UpdateTrackingData(ref TrackingData p_data)
        {
            m_headPosition.Set(p_data.m_headPositionX * (Settings.Mirrored ? -1f : 1f), p_data.m_headPositionY, p_data.m_headPositionZ);
            m_headRotation.Set(p_data.m_headRotationX, p_data.m_headRotationY * (Settings.Mirrored ? -1f : 1f), p_data.m_headRotationZ * (Settings.Mirrored ? -1f : 1f), p_data.m_headRotationW);
            m_gazeDirection.Set(Settings.Mirrored ? (1f - p_data.m_gazeX) : p_data.m_gazeX, p_data.m_gazeY);
            m_blinkProgress = p_data.m_blink;

            float l_weight = Mathf.Clamp01(Mathf.InverseLerp(0.25f, 1f, Mathf.Abs(p_data.m_mouthShape)));
            m_lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Jaw_Open] = p_data.m_mouthOpen;
            m_lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Pout] = ((p_data.m_mouthShape > 0f) ? l_weight : 0f);
            m_lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Smile_Left] = ((p_data.m_mouthShape < 0f) ? l_weight : 0f);
            m_lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Smile_Right] = ((p_data.m_mouthShape < 0f) ? l_weight : 0f);
        }

        void OnLookIKPostUpdate()
        {
            if(Settings.Enabled && Settings.HeadTracking && (m_headBone != null))
            {
                m_lastHeadRotation = Quaternion.Slerp(m_lastHeadRotation, m_avatarDescriptor.transform.rotation * (m_headRotation * m_bindRotation), m_smoothing);

                if(!PlayerSetup.Instance.IsEmotePlaying)
                    m_headBone.rotation = m_lastHeadRotation;
            }
        }

        // Game events
        internal void OnAvatarSetup(CVRAvatar p_avatar)
        {
            try
            {
                m_camera = PlayerSetup.Instance.activeCam.transform;
                m_avatarDescriptor = PlayerSetup.Instance.AvatarObject.GetComponent<CVRAvatar>();

                if(PlayerSetup.Instance.Animator.isHuman)
                {
                    IKSystem.Instance.SetAvatarPose(IKSystem.AvatarPose.TPose);
                    PlayerSetup.Instance.AvatarTransform.localPosition = Vector3.zero;
                    PlayerSetup.Instance.AvatarTransform.localRotation = Quaternion.identity;

                    m_headBone = PlayerSetup.Instance.Animator.GetBoneTransform(HumanBodyBones.Head);
                    if(m_headBone != null)
                        m_bindRotation = Quaternion.Inverse(m_avatarDescriptor.transform.rotation) * m_headBone.rotation;

                    m_lookIK = PlayerSetup.Instance.AvatarObject.GetComponent<LookAtIK>();
                    if(m_lookIK != null)
                        m_lookIK.onPostSolverUpdate.AddListener(this.OnLookIKPostUpdate);
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
        void OnAvatarClear(CVRAvatar p_avatar)
        {
            try
            {
                m_avatarDescriptor = null;
                m_lookIK = null;
                m_headBone = null;
                m_lastHeadRotation = Quaternion.identity;
                m_bindRotation = Quaternion.identity;
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
        void OnAvatarReuse()
        {
            m_camera = PlayerSetup.Instance.activeCam.transform;

            m_lookIK = PlayerSetup.Instance.AvatarObject.GetComponent<LookAtIK>();
            if(m_lookIK != null)
                m_lookIK.onPostSolverUpdate.AddListener(this.OnLookIKPostUpdate);
        }

        void OnEyeControllerUpdate(EyeMovementController p_component)
        {
            if(this.enabled && Settings.Enabled)
            {
                // Gaze
                if(Settings.EyeTracking && (m_camera != null))
                {
                    p_component.manualViewTarget = true;
                    p_component.targetViewPosition = m_camera.position + m_camera.rotation * new Vector3((m_gazeDirection.x - 0.5f) * 2f, (m_gazeDirection.y - 0.5f) * 2f, 1f);
                }

                // Blink
                if(Settings.Blinking)
                {
                    p_component.manualBlinking = true;
                    p_component.blinkProgressLeft = m_blinkProgress;
                    p_component.blinkProgressRight = m_blinkProgress;
                }
            }
        }

        void UpdateFaceTracking(CVRFaceTracking p_component, GameEvents.EventResult p_result)
        {
            if(this.enabled && Settings.Enabled && Settings.FaceTracking && p_component.isLocal && p_component.UseFacialTracking)
            {
                if(!m_lipDataSent)
                {
                    FaceTrackingManager.Instance.SubmitNewFacialData(m_lipData);
                    m_lipDataSent = true;
                }
                p_component.LipSyncWasUpdated = true;
                p_component.UpdateShapesLocal_Private();

                p_result.m_result |= true;
            }
        }

        void OnVRModeSwitch(bool p_state)
        {
            try
            {
                this.enabled = !Utils.IsInVR();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        // Settings
        void OnEnabledOrHeadTrackingChanged(bool p_state)
        {
            if(Settings.Enabled && Settings.HeadTracking)
                m_lastHeadRotation = ((m_headBone != null) ? m_headBone.rotation : m_bindRotation);
        }
        void OnSmoothingChanged(float p_value)
        {
            m_smoothing = 1f - Mathf.Clamp(p_value, 0f, 0.99f);
        }
    }
}

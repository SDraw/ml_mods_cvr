using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Core.Player.EyeMovement;
using ABI_RC.Systems.FaceTracking;
using RootMotion.FinalIK;
using System;
using System.Reflection;
using UnityEngine;
using ViveSR.anipal.Lip;

namespace ml_dht
{
    [DisallowMultipleComponent]
    class HeadTracked : MonoBehaviour
    {
        static FieldInfo ms_emotePlaying = typeof(PlayerSetup).GetField("_emotePlaying", BindingFlags.NonPublic | BindingFlags.Instance);

        bool m_enabled = false;
        bool m_headTracking = true;
        float m_smoothing = 0.5f;

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
        void Start()
        {
            OnEnabledChanged(Settings.Enabled);
            OnHeadTrackingChanged(Settings.HeadTracking);
            OnSmoothingChanged(Settings.Smoothing);

            Settings.OnEnabledChanged.AddHandler(this.OnEnabledChanged);
            Settings.OnHeadTrackingChanged.AddHandler(this.OnHeadTrackingChanged);
            Settings.OnSmoothingChanged.AddHandler(this.OnSmoothingChanged);

            GameEvents.OnAvatarClear.AddHandler(this.OnAvatarClear);
            GameEvents.OnAvatarSetup.AddHandler(this.OnAvatarSetup);
            GameEvents.OnAvatarReuse.AddHandler(this.OnAvatarReuse);
            GameEvents.OnEyeControllerUpdate.AddHandler(this.OnEyeControllerUpdate);
            GameEvents.OnFaceTrackingUpdate.AddHandler(this.UpdateFaceTracking);
        }

        void OnDestroy()
        {
            Settings.OnEnabledChanged.RemoveHandler(this.OnEnabledChanged);
            Settings.OnHeadTrackingChanged.RemoveHandler(this.OnHeadTrackingChanged);
            Settings.OnSmoothingChanged.RemoveHandler(this.OnSmoothingChanged);

            GameEvents.OnAvatarClear.RemoveHandler(this.OnAvatarClear);
            GameEvents.OnAvatarSetup.RemoveHandler(this.OnAvatarSetup);
            GameEvents.OnAvatarReuse.RemoveHandler(this.OnAvatarReuse);
            GameEvents.OnEyeControllerUpdate.RemoveHandler(this.OnEyeControllerUpdate);
            GameEvents.OnFaceTrackingUpdate.RemoveHandler(this.UpdateFaceTracking);
        }

        void Update()
        {
            if(m_lipDataSent)
                m_lipDataSent = false;
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
            if(m_enabled && m_headTracking && (m_headBone != null))
            {
                m_lastHeadRotation = Quaternion.Slerp(m_lastHeadRotation, m_avatarDescriptor.transform.rotation * (m_headRotation * m_bindRotation), m_smoothing);

                if(!(bool)ms_emotePlaying.GetValue(PlayerSetup.Instance))
                    m_headBone.rotation = m_lastHeadRotation;
            }
        }

        // Game events
        internal void OnAvatarSetup()
        {
            Utils.SetAvatarTPose();

            m_camera = PlayerSetup.Instance.GetActiveCamera().transform;
            m_avatarDescriptor = PlayerSetup.Instance._avatar.GetComponent<CVRAvatar>();
            m_headBone = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Head);
            m_lookIK = PlayerSetup.Instance._avatar.GetComponent<LookAtIK>();

            if(m_headBone != null)
                m_bindRotation = Quaternion.Inverse(m_avatarDescriptor.transform.rotation) * m_headBone.rotation;

            if(m_lookIK != null)
                m_lookIK.onPostSolverUpdate.AddListener(this.OnLookIKPostUpdate);

        }
        void OnAvatarClear()
        {
            m_avatarDescriptor = null;
            m_lookIK = null;
            m_headBone = null;
            m_lastHeadRotation = Quaternion.identity;
            m_bindRotation = Quaternion.identity;
        }
        void OnAvatarReuse()
        {
            m_camera = PlayerSetup.Instance.GetActiveCamera().transform;
            m_lookIK = PlayerSetup.Instance._avatar.GetComponent<LookAtIK>();
            if(m_lookIK != null)
                m_lookIK.onPostSolverUpdate.AddListener(this.OnLookIKPostUpdate);
        }

        void OnEyeControllerUpdate(EyeMovementController p_component)
        {
            if(m_enabled)
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
                    p_component.blinkProgress = m_blinkProgress;
                }
            }
        }

        void UpdateFaceTracking(CVRFaceTracking p_component, GameEvents.EventResult p_result)
        {
            if(p_component.isLocal && p_component.UseFacialTracking && m_enabled && Settings.FaceTracking)
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

        // Settings
        void OnEnabledChanged(bool p_state)
        {
            if(m_enabled != p_state)
            {
                m_enabled = p_state;
                TryRestoreHeadRotation();
            }
        }
        void OnHeadTrackingChanged(bool p_state)
        {
            if(m_headTracking != p_state)
            {
                m_headTracking = p_state;
                TryRestoreHeadRotation();
            }
        }
        void OnSmoothingChanged(float p_value)
        {
            m_smoothing = 1f - Mathf.Clamp(p_value, 0f, 0.99f);
        }

        // Arbitrary
        void TryRestoreHeadRotation()
        {
            if(m_enabled && m_headTracking)
                m_lastHeadRotation = ((m_headBone != null) ? m_headBone.rotation : m_bindRotation);
        }
    }
}

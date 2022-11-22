using ABI.CCK.Components;
using ABI_RC.Core.Player;
using RootMotion.FinalIK;
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
        bool m_blinking = true;
        bool m_eyeTracking = true;
        float m_smoothing = 0.5f;
        bool m_mirrored = false;
        bool m_faceOverride = true;

        CVRAvatar m_avatarDescriptor = null;
        LookAtIK m_lookIK = null;
        Transform m_headBone = null;

        Vector3 m_headPosition;
        Quaternion m_headRotation;
        Vector2 m_gazeDirection;
        float m_blinkProgress = 0f;
        Vector2 m_mouthShapes;
        float m_eyebrowsProgress = 0f;

        Quaternion m_bindRotation;
        Quaternion m_lastHeadRotation;

        void Start()
        {
            Settings.EnabledChange += this.SetEnabled;
            Settings.HeadTrackingChange += this.SetHeadTracking;
            Settings.EyeTrackingChange += this.SetEyeTracking;
            Settings.BlinkingChange += this.SetBlinking;
            Settings.SmoothingChange += this.SetSmoothing;
            Settings.MirroredChange += this.SetMirrored;
            Settings.FaceOverrideChange += this.SetFaceOverride;
        }

        void OnDestroy()
        {
            Settings.EnabledChange -= this.SetEnabled;
            Settings.HeadTrackingChange -= this.SetHeadTracking;
            Settings.EyeTrackingChange -= this.SetEyeTracking;
            Settings.BlinkingChange -= this.SetBlinking;
            Settings.SmoothingChange -= this.SetSmoothing;
            Settings.MirroredChange -= this.SetMirrored;
            Settings.FaceOverrideChange -= this.SetFaceOverride;
        }

        public void UpdateTrackingData(ref TrackingData p_data)
        {
            m_headPosition.Set(p_data.m_headPositionX * (m_mirrored ? -1f : 1f), p_data.m_headPositionY, p_data.m_headPositionZ);
            m_headRotation.Set(p_data.m_headRotationX, p_data.m_headRotationY * (m_mirrored ? -1f : 1f), p_data.m_headRotationZ * (m_mirrored ? -1f : 1f), p_data.m_headRotationW);
            m_gazeDirection.Set(m_mirrored ? (1f - p_data.m_gazeX) : p_data.m_gazeX, p_data.m_gazeY);
            m_blinkProgress = p_data.m_blink;
            m_mouthShapes.Set(p_data.m_mouthOpen, p_data.m_mouthShape);
            m_eyebrowsProgress = p_data.m_brows;
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

        public void OnEyeControllerUpdate(CVREyeController p_component)
        {
            if(m_enabled)
            {
                // Gaze
                if(m_eyeTracking)
                {
                    Transform l_camera = PlayerSetup.Instance.GetActiveCamera().transform;

                    p_component.manualViewTarget = true;
                    p_component.targetViewPosition = l_camera.position + l_camera.rotation * new Vector3((m_gazeDirection.x - 0.5f) * 2f, (m_gazeDirection.y - 0.5f) * 2f, 1f);
                }

                // Blink
                if(m_blinking)
                {
                    p_component.manualBlinking = true;
                    p_component.blinkProgress = m_blinkProgress;
                }
            }
        }

        public void OnFaceTrackingUpdate(CVRFaceTracking p_component)
        {
            if(m_enabled && m_faceOverride)
            {
                if(m_avatarDescriptor != null)
                    m_avatarDescriptor.useVisemeLipsync = false;

                float l_weight = Mathf.Clamp(Mathf.InverseLerp(0.25f, 1f, Mathf.Abs(m_mouthShapes.y)), 0f, 1f) * 100f;

                p_component.BlendShapeValues[(int)LipShape_v2.Jaw_Open] = m_mouthShapes.x * 100f;
                p_component.BlendShapeValues[(int)LipShape_v2.Mouth_Pout] = ((m_mouthShapes.y > 0f) ? l_weight : 0f);
                p_component.BlendShapeValues[(int)LipShape_v2.Mouth_Smile_Left] = ((m_mouthShapes.y < 0f) ? l_weight : 0f);
                p_component.BlendShapeValues[(int)LipShape_v2.Mouth_Smile_Right] = ((m_mouthShapes.y < 0f) ? l_weight : 0f);
                p_component.LipSyncWasUpdated = true;
                p_component.UpdateLipShapes();
            }
        }

        public void OnSetupAvatar()
        {
            m_avatarDescriptor = PlayerSetup.Instance._avatar.GetComponent<CVRAvatar>();
            m_headBone = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Head);
            m_lookIK = PlayerSetup.Instance._avatar.GetComponent<LookAtIK>();

            if(m_headBone != null)
                m_bindRotation = (m_avatarDescriptor.transform.GetMatrix().inverse * m_headBone.GetMatrix()).rotation;

            if(m_lookIK != null)
                m_lookIK.solver.OnPostUpdate += this.OnLookIKPostUpdate;

        }
        public void OnAvatarClear()
        {
            m_avatarDescriptor = null;
            m_lookIK = null;
            m_headBone = null;
            m_lastHeadRotation = Quaternion.identity;
            m_bindRotation = Quaternion.identity;
        }

        public void SetEnabled(bool p_state)
        {
            if(m_enabled != p_state)
            {
                m_enabled = p_state;
                if(m_enabled && m_headTracking)
                    m_lastHeadRotation = ((m_headBone != null) ? m_headBone.rotation : m_bindRotation);
            }
        }
        public void SetHeadTracking(bool p_state)
        {
            if(m_headTracking != p_state)
            {
                m_headTracking = p_state;
                if(m_enabled && m_headTracking)
                    m_lastHeadRotation = ((m_headBone != null) ? m_headBone.rotation : m_bindRotation);
            }
        }
        public void SetEyeTracking(bool p_state)
        {
            m_eyeTracking = p_state;
        }
        public void SetBlinking(bool p_state)
        {
            m_blinking = p_state;
        }
        public void SetSmoothing(float p_value)
        {
            m_smoothing = 1f - Mathf.Clamp(p_value, 0f, 0.99f);
        }
        public void SetMirrored(bool p_state)
        {
            m_mirrored = p_state;
        }
        public void SetFaceOverride(bool p_state)
        {
            m_faceOverride = p_state;
        }
    }
}

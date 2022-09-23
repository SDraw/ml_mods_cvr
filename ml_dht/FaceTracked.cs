using ABI.CCK.Components;
using ABI_RC.Core.Player;
using UnityEngine;

namespace ml_dht
{
    class FaceTracked : MonoBehaviour
    {
        bool m_enabled = true;
        float m_smoothing = 0.5f;
        bool m_mirrored = false;
        bool m_faceOverride = true;

        CVRAvatar m_avatarDescriptior = null;
        RootMotion.FinalIK.LookAtIK m_lookIK = null;
        Transform m_camera = null;
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
            m_camera = PlayerSetup.Instance.desktopCamera.transform;
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

        public void OnEyeControllerUpdate()
        {
            if(m_enabled)
            {
                // Gaze
                PlayerSetup.Instance.eyeMovement.manualViewTarget = true;
                PlayerSetup.Instance.eyeMovement.targetViewPosition = m_camera.position + m_camera.rotation * new Vector3((m_gazeDirection.x - 0.5f) * -2f, (m_gazeDirection.y - 0.5f) * 2f, 1f);

                // Blink
                PlayerSetup.Instance.eyeMovement.manualBlinking = true;
                PlayerSetup.Instance.eyeMovement.blinkProgress = m_blinkProgress;
            }
        }

        void OnLookIKPostUpdate()
        {
            if(m_enabled && (m_headBone != null))
            {
                m_lastHeadRotation = Quaternion.Slerp(m_lastHeadRotation, m_headRotation * m_bindRotation, m_smoothing);
                m_headBone.localRotation = m_lastHeadRotation;
            }
        }

        public void OnFaceTrackingUpdate(CVRFaceTracking p_component)
        {
            if(m_enabled && m_faceOverride)
            {
                if(m_avatarDescriptior != null)
                    m_avatarDescriptior.useVisemeLipsync = false;

                p_component.BlendShapeValues[(int)ViveSR.anipal.Lip.LipShape_v2.Jaw_Open] = m_mouthShapes.x * 100f;

                if(m_mouthShapes.y < 0f)
                {
                    float l_weight = Mathf.Clamp(Mathf.InverseLerp(0.25f, 1f, -m_mouthShapes.y), 0f, 1f) * 100f;
                    p_component.BlendShapeValues[(int)ViveSR.anipal.Lip.LipShape_v2.Mouth_Pout] = 0f;
                    p_component.BlendShapeValues[(int)ViveSR.anipal.Lip.LipShape_v2.Mouth_Smile_Left] = l_weight;
                    p_component.BlendShapeValues[(int)ViveSR.anipal.Lip.LipShape_v2.Mouth_Smile_Right] = l_weight;
                }
                if(m_mouthShapes.y > 0f)
                {
                    float l_weight = Mathf.Clamp(Mathf.InverseLerp(0.25f, 1f, m_mouthShapes.y), 0f, 1f) * 100f;
                    p_component.BlendShapeValues[(int)ViveSR.anipal.Lip.LipShape_v2.Mouth_Pout] = l_weight;
                    p_component.BlendShapeValues[(int)ViveSR.anipal.Lip.LipShape_v2.Mouth_Smile_Left] = 0f;
                    p_component.BlendShapeValues[(int)ViveSR.anipal.Lip.LipShape_v2.Mouth_Smile_Right] = 0f;
                }

                p_component.LipSyncWasUpdated = true;
                p_component.UpdateLipShapes();
            }
        }

        public void OnSetupAvatarGeneral()
        {
            m_avatarDescriptior = PlayerSetup.Instance._avatar.GetComponent<CVRAvatar>();
            m_headBone = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Head);
            m_lookIK = PlayerSetup.Instance._avatar.GetComponent<RootMotion.FinalIK.LookAtIK>();

            if(m_headBone != null)
                m_bindRotation = m_headBone.localRotation;

            if(m_lookIK != null)
                m_lookIK.solver.OnPostUpdate += this.OnLookIKPostUpdate;

        }
        public void OnAvatarClear()
        {
            m_avatarDescriptior = null;
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
                if(m_enabled)
                    m_lastHeadRotation = m_bindRotation;
            }
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

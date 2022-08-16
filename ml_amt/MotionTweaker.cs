using ABI_RC.Core.Player;
using System.Collections.Generic;
using UnityEngine;

namespace ml_amt
{
    class MotionTweaker : MonoBehaviour
    {
        enum ParameterType
        {
            Upright
        }
        enum ParameterSyncType
        {
            Local,
            Synced
        }

        struct AdditionalParameterInfo
        {
            public ParameterType m_type;
            public ParameterSyncType m_sync;
            public string m_name;
            public int m_hash; // For local only
        }

        static readonly Vector4 ms_pointVector = new Vector4(0f, 0f, 0f, 1f);

        RootMotion.FinalIK.VRIK m_vrIk = null;

        bool m_ready = false;

        bool m_standing = true;
        float m_currentUpright = 1f;
        float m_locomotionWeight = 1f;
        float m_crouchLimit = 0.65f;
        bool m_customCrouchLimit = false;

        readonly List<AdditionalParameterInfo> m_parameters = null;

        public MotionTweaker()
        {
            m_parameters = new List<AdditionalParameterInfo>();
        }

        void Start()
        {
            if(PlayerSetup.Instance._inVr)
                PlayerSetup.Instance.avatarSetupCompleted.AddListener(this.OnAvatarSetup);
        }

        void Update()
        {
            if(m_ready)
            {
                // Update upright
                Matrix4x4 l_hmdMatrix = PlayerSetup.Instance.transform.GetMatrix().inverse * (PlayerSetup.Instance._inVr ? PlayerSetup.Instance.vrHeadTracker.transform.GetMatrix() : PlayerSetup.Instance.desktopCameraRig.transform.GetMatrix());
                float l_currentHeight = Mathf.Clamp((l_hmdMatrix * ms_pointVector).y, 0f, float.MaxValue);
                float l_avatarViewHeight = Mathf.Clamp(PlayerSetup.Instance.GetViewPointHeight() * PlayerSetup.Instance._avatar.transform.localScale.y, 0f, float.MaxValue);
                m_currentUpright = Mathf.Clamp((((l_currentHeight > 0f) && (l_avatarViewHeight > 0f)) ? (l_currentHeight / l_avatarViewHeight) : 0f), 0f, 1f);
                m_standing = (m_currentUpright > m_crouchLimit);

                if((m_vrIk != null) && m_vrIk.enabled && !PlayerSetup.Instance._movementSystem.sitting && (PlayerSetup.Instance._movementSystem.movementVector.magnitude <= Mathf.Epsilon) && !PlayerSetup.Instance.fullBodyActive)
                {
                    m_locomotionWeight = Mathf.Lerp(m_locomotionWeight, m_standing ? 1f : 0f, 0.5f);
                    m_vrIk.solver.locomotion.weight = m_locomotionWeight;
                }

                if(m_parameters.Count > 0)
                {
                    foreach(AdditionalParameterInfo l_param in m_parameters)
                    {
                        switch(l_param.m_type)
                        {
                            case ParameterType.Upright:
                            {
                                switch(l_param.m_sync)
                                {
                                    case ParameterSyncType.Local:
                                        PlayerSetup.Instance._animator.SetFloat(l_param.m_hash, m_currentUpright);
                                        break;
                                    case ParameterSyncType.Synced:
                                        PlayerSetup.Instance.changeAnimatorParam(l_param.m_name, m_currentUpright);
                                        break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        public void OnAvatarClear()
        {
            m_vrIk = null;
            m_ready = false;
            m_standing = true;
            m_parameters.Clear();
            m_locomotionWeight = 1f;
            m_crouchLimit = 0.65f;
            m_customCrouchLimit = false;
        }

        public void OnAvatarSetup()
        {
            m_vrIk = PlayerSetup.Instance._avatar.GetComponent<RootMotion.FinalIK.VRIK>();

            // Parse animator parameters
            AnimatorControllerParameter[] l_params = PlayerSetup.Instance._animator.parameters;
            ParameterType[] l_enumParams = (ParameterType[])System.Enum.GetValues(typeof(ParameterType));

            foreach(var l_param in l_params)
            {
                foreach(var l_enumParam in l_enumParams)
                {
                    if(l_param.name.Contains(l_enumParam.ToString()) && (m_parameters.FindIndex(p => p.m_type == l_enumParam) == -1))
                    {
                        bool l_local = (l_param.name[0] == '#');

                        m_parameters.Add(new AdditionalParameterInfo
                        {
                            m_type = l_enumParam,
                            m_sync = (l_local ? ParameterSyncType.Local : ParameterSyncType.Synced),
                            m_name = l_param.name,
                            m_hash = (l_local ? l_param.nameHash : 0)
                        });

                        break;
                    }
                }
            }

            Transform l_customLimit = PlayerSetup.Instance._avatar.transform.Find("CrouchLimit");
            m_customCrouchLimit = (l_customLimit != null);
            m_crouchLimit = m_customCrouchLimit ? Mathf.Clamp(l_customLimit.localPosition.y, 0f, 1f) : Settings.CrouchLimit;

            m_ready = true;
        }

        public void SetCrouchLimit(float p_value)
        {
            if(!m_customCrouchLimit)
                m_crouchLimit = Mathf.Clamp(p_value, 0f, 1f);
        }
    }
}

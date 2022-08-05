using ABI_RC.Core;
using ABI_RC.Core.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ml_aap
{
    class ParametersHandler : MonoBehaviour
    {
        enum AdditionalParameter
        {
            Upright,
            Viseme,
            Voice,
            Muted,
            InVR,
            InHmd,
            InFBT,
            Zoom
        }
        enum AdditionalParameterSync
        {
            Local,
            Synced
        }

        struct AdditionalParameterInfo
        {
            public AdditionalParameter m_type;
            public AdditionalParameterSync m_sync;
            public string m_name;
            public int m_hash; // For local only
        }

        static readonly Vector4 ms_pointVector = new Vector4(0f, 0f, 0f, 1f);
        static readonly System.Reflection.FieldInfo ms_visemeWeights = typeof(CVRVisemeController).GetField("visemeWeights", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        readonly List<AdditionalParameterInfo> m_parameters = null;
        bool m_active = false;

        CVRVisemeController m_visemeController = null;

        public ParametersHandler()
        {
            m_parameters = new List<AdditionalParameterInfo>();
        }

        void Update()
        {
            if(m_active)
            {
                foreach(AdditionalParameterInfo l_param in m_parameters)
                {
                    switch(l_param.m_type)
                    {
                        case AdditionalParameter.Upright:
                        {
                            Matrix4x4 l_hmdMatrix = PlayerSetup.Instance.transform.GetMatrix().inverse * (PlayerSetup.Instance._inVr ? PlayerSetup.Instance.vrHeadTracker.transform.GetMatrix() : PlayerSetup.Instance.desktopCameraRig.transform.GetMatrix());
                            float l_currentHeight = Mathf.Clamp((l_hmdMatrix * ms_pointVector).y, 0f, float.MaxValue);
                            float l_avatarViewHeight = Mathf.Clamp(PlayerSetup.Instance.GetViewPointHeight() * PlayerSetup.Instance.GetAvatarScale().y, 0f, float.MaxValue);
                            float l_currentUpright = Mathf.Clamp((((l_currentHeight > 0f) && (l_avatarViewHeight > 0f)) ? (l_currentHeight / l_avatarViewHeight) : 0f), 0f, 1f);

                            switch(l_param.m_sync)
                            {
                                case AdditionalParameterSync.Local:
                                    PlayerSetup.Instance._animator.SetFloat(l_param.m_hash, l_currentUpright);
                                    break;
                                case AdditionalParameterSync.Synced:
                                    PlayerSetup.Instance.changeAnimatorParam(l_param.m_name, l_currentUpright);
                                    break;
                            }
                        }
                        break;

                        case AdditionalParameter.Viseme:
                        {
                            float[] l_weights = (float[])ms_visemeWeights?.GetValue(m_visemeController);
                            if(l_weights != null)
                            {
                                int l_index = 0;
                                float l_maxWeight = 0f;

                                for(int i = 0; i < l_weights.Length; i++)
                                {
                                    if(l_maxWeight < l_weights[i])
                                    {
                                        l_maxWeight = l_weights[i];
                                        l_index = i;
                                    }
                                }

                                switch(l_param.m_sync)
                                {
                                    case AdditionalParameterSync.Local:
                                        PlayerSetup.Instance._animator.SetInteger(l_param.m_hash, l_index);
                                        break;
                                    case AdditionalParameterSync.Synced:
                                        PlayerSetup.Instance.changeAnimatorParam(l_param.m_name, l_index);
                                        break;
                                }
                            }
                        }
                        break;

                        case AdditionalParameter.Voice:
                        {
                            switch(l_param.m_sync)
                            {
                                case AdditionalParameterSync.Local:
                                    PlayerSetup.Instance._animator.SetFloat(l_param.m_hash, m_visemeController.visemeLoudness);
                                    break;
                                case AdditionalParameterSync.Synced:
                                    PlayerSetup.Instance.changeAnimatorParam(l_param.m_name, m_visemeController.visemeLoudness);
                                    break;
                            }
                        }
                        break;

                        case AdditionalParameter.InVR:
                        {
                            switch(l_param.m_sync)
                            {
                                case AdditionalParameterSync.Local:
                                    PlayerSetup.Instance._animator.SetBool(l_param.m_hash, PlayerSetup.Instance._inVr);
                                    break;
                                case AdditionalParameterSync.Synced:
                                    PlayerSetup.Instance.changeAnimatorParam(l_param.m_name, PlayerSetup.Instance._inVr ? 1f : 0f);
                                    break;
                            }
                        }
                        break;

                        case AdditionalParameter.InHmd:
                        {
                            switch(l_param.m_sync)
                            {
                                case AdditionalParameterSync.Local:
                                    PlayerSetup.Instance._animator.SetBool(l_param.m_hash, PlayerSetup.Instance._trackerManager.headsetOnHead);
                                    break;
                                case AdditionalParameterSync.Synced:
                                    PlayerSetup.Instance.changeAnimatorParam(l_param.m_name, PlayerSetup.Instance._trackerManager.headsetOnHead ? 1f : 0f);
                                    break;
                            }
                        } break;

                        case AdditionalParameter.InFBT:
                        {
                            switch(l_param.m_sync)
                            {
                                case AdditionalParameterSync.Local:
                                    PlayerSetup.Instance._animator.SetBool(l_param.m_hash, PlayerSetup.Instance.fullBodyActive);
                                    break;
                                case AdditionalParameterSync.Synced:
                                    PlayerSetup.Instance.changeAnimatorParam(l_param.m_name, PlayerSetup.Instance.fullBodyActive ? 1f : 0f);
                                    break;
                            }
                        } break;

                        case AdditionalParameter.Muted:
                        {
                            switch(l_param.m_sync)
                            {
                                case AdditionalParameterSync.Local:
                                    PlayerSetup.Instance._animator.SetBool(l_param.m_hash, RootLogic.Instance.comms.IsMuted);
                                    break;
                                case AdditionalParameterSync.Synced:
                                    PlayerSetup.Instance.changeAnimatorParam(l_param.m_name, RootLogic.Instance.comms.IsMuted ? 1f : 0f);
                                    break;
                            }
                        }
                        break;

                        case AdditionalParameter.Zoom:
                        {
                            switch(l_param.m_sync)
                            {
                                case AdditionalParameterSync.Local:
                                    PlayerSetup.Instance._animator.SetFloat(l_param.m_hash, CVR_DesktopCameraController.currentZoomProgress);
                                    break;
                                case AdditionalParameterSync.Synced:
                                    PlayerSetup.Instance.changeAnimatorParam(l_param.m_name, CVR_DesktopCameraController.currentZoomProgress);
                                    break;
                            }
                        } break;
                    }
                }
            }
        }

        public void OnAvatarClear()
        {
            m_parameters.Clear();
            m_active = false;
            m_visemeController = null;
        }

        public void OnAvatarSetup()
        {
            m_visemeController = PlayerSetup.Instance._animator.GetComponent<CVRVisemeController>();

            AnimatorControllerParameter[] l_params = PlayerSetup.Instance._animator.parameters;
            AdditionalParameter[] l_enumParams = (AdditionalParameter[])Enum.GetValues(typeof(AdditionalParameter));

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
                            m_sync = (l_local ? AdditionalParameterSync.Local : AdditionalParameterSync.Synced),
                            m_name = l_param.name,
                            m_hash = (l_local ? l_param.nameHash : 0)
                        });

                        break;
                    }
                }
            }

            m_active = (m_parameters.Count > 0);
        }
    }
}

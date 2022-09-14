using ABI_RC.Core.Player;
using RootMotion.FinalIK;
using System.Collections.Generic;
using UnityEngine;

namespace ml_amt
{
    class MotionTweaker : MonoBehaviour
    {
        static System.Reflection.FieldInfo ms_rootVelocity = typeof(IKSolverVR).GetField("rootVelocity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

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

        enum PoseState
        {
            Standing = 0,
            Crouching,
            Proning
        }

        static readonly Vector4 ms_pointVector = new Vector4(0f, 0f, 0f, 1f);

        VRIK m_vrIk = null;
        float m_locomotionWeight = 1f; // Original weight

        bool m_avatarReady = false;
        bool m_compatibleAvatar = false;

        bool m_ikOverride = true;
        float m_currentUpright = 1f;
        PoseState m_poseState = PoseState.Standing;

        float m_crouchLimit = 0.65f;
        bool m_customCrouchLimit = false;

        bool m_detectPose = true;
        float m_proneLimit = 0.3f;
        bool m_customProneLimit = false;

        bool m_customLocomotionOffset = false;
        Vector3 m_locomotionOffset = Vector3.zero;

        readonly List<AdditionalParameterInfo> m_parameters = null;

        public MotionTweaker()
        {
            m_parameters = new List<AdditionalParameterInfo>();
        }

        void Update()
        {
            if(m_avatarReady)
            {
                // Update upright
                Matrix4x4 l_hmdMatrix = PlayerSetup.Instance.transform.GetMatrix().inverse * (PlayerSetup.Instance._inVr ? PlayerSetup.Instance.vrHeadTracker.transform.GetMatrix() : PlayerSetup.Instance.desktopCameraRig.transform.GetMatrix());
                float l_currentHeight = Mathf.Clamp((l_hmdMatrix * ms_pointVector).y, 0f, float.MaxValue);
                float l_avatarViewHeight = Mathf.Clamp(PlayerSetup.Instance.GetViewPointHeight() * PlayerSetup.Instance._avatar.transform.localScale.y, 0f, float.MaxValue);
                m_currentUpright = Mathf.Clamp((((l_currentHeight > 0f) && (l_avatarViewHeight > 0f)) ? (l_currentHeight / l_avatarViewHeight) : 0f), 0f, 1f);
                PoseState l_poseState = (m_currentUpright <= m_proneLimit) ? PoseState.Proning : ((m_currentUpright <= m_crouchLimit) ? PoseState.Crouching : PoseState.Standing);

                if((m_vrIk != null) && m_vrIk.enabled)
                {
                    if(m_ikOverride && (m_poseState != l_poseState) && (l_poseState == PoseState.Standing))
                        ms_rootVelocity.SetValue(m_vrIk.solver, Vector3.zero);

                    if(m_detectPose && !m_compatibleAvatar && !PlayerSetup.Instance.fullBodyActive)
                    {
                        switch(l_poseState)
                        {
                            case PoseState.Standing:
                            {
                                PlayerSetup.Instance._movementSystem.ChangeCrouch(false);
                                PlayerSetup.Instance._movementSystem.ChangeProne(false);
                                PlayerSetup.Instance.animatorManager.SetAnimatorParameterBool("Crouching", false); // Forced to stop transitioning to standing locomotion
                                PlayerSetup.Instance.animatorManager.SetAnimatorParameterBool("Prone", false);  // Forced to stop transitioning to standing locomotion
                            }
                            break;

                            case PoseState.Crouching:
                                PlayerSetup.Instance._movementSystem.ChangeCrouch(true);
                                PlayerSetup.Instance.animatorManager.SetAnimatorParameterBool("Crouching", true);
                                PlayerSetup.Instance.animatorManager.SetAnimatorParameterBool("Prone", false);
                                break;

                            case PoseState.Proning:
                            {
                                PlayerSetup.Instance._movementSystem.ChangeProne(true);
                                PlayerSetup.Instance.animatorManager.SetAnimatorParameterBool("Crouching", false);
                                PlayerSetup.Instance.animatorManager.SetAnimatorParameterBool("Prone", true);
                            }
                            break;
                        }
                    }
                }

                m_poseState = l_poseState;

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
            m_avatarReady = false;
            m_compatibleAvatar = false;
            m_vrIk = null;
            m_poseState = PoseState.Standing;
            m_parameters.Clear();
            m_customCrouchLimit = false;
            m_customProneLimit = false;
            m_customLocomotionOffset = false;
            m_locomotionOffset = Vector3.zero;
        }

        public void OnSetupAvatarGeneral()
        {
            m_vrIk = PlayerSetup.Instance._avatar.GetComponent<VRIK>();

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

            m_compatibleAvatar = m_parameters.Exists(p => p.m_name.Contains("Upright"));

            Transform l_customTransform = PlayerSetup.Instance._avatar.transform.Find("CrouchLimit");
            m_customCrouchLimit = (l_customTransform != null);
            m_crouchLimit = m_customCrouchLimit ? Mathf.Clamp(l_customTransform.localPosition.y, 0f, 1f) : Settings.CrouchLimit;

            l_customTransform = PlayerSetup.Instance._avatar.transform.Find("ProneLimit");
            m_customProneLimit = (l_customTransform != null);
            m_proneLimit = m_customProneLimit ? Mathf.Clamp(l_customTransform.localPosition.y, 0f, 1f) : Settings.ProneLimit;

            l_customTransform = PlayerSetup.Instance._avatar.transform.Find("LocomotionOffset");
            m_customLocomotionOffset = (l_customTransform != null);
            m_locomotionOffset = m_customLocomotionOffset ? l_customTransform.localPosition : Vector3.zero;

            // Apply VRIK tweaks
            if(m_vrIk != null)
            {
                if(m_customLocomotionOffset)
                    m_vrIk.solver.locomotion.offset = m_locomotionOffset;

                m_vrIk.solver.OnPreUpdate += this.OnIKPreUpdate;
                m_vrIk.solver.OnPostUpdate += this.OnIKPostUpdate;
            }

            m_avatarReady = true;
        }

        void OnIKPreUpdate()
        {
            if(m_ikOverride)
            {
                m_locomotionWeight = m_vrIk.solver.locomotion.weight;
                if(m_poseState != PoseState.Standing)
                    m_vrIk.solver.locomotion.weight = 0f;
            }
        }

        void OnIKPostUpdate()
        {
            if(m_ikOverride)
                m_vrIk.solver.locomotion.weight = m_locomotionWeight;
        }

        public void SetIKOverride(bool p_state)
        {
            m_ikOverride = p_state;
        }
        public void SetCrouchLimit(float p_value)
        {
            if(!m_customCrouchLimit)
                m_crouchLimit = Mathf.Clamp(p_value, 0f, 1f);
        }
        public void SetDetectPose(bool p_state)
        {
            m_detectPose = p_state;

            if(!m_detectPose && m_avatarReady && !m_compatibleAvatar && PlayerSetup.Instance._inVr)
            {
                PlayerSetup.Instance._movementSystem.ChangeCrouch(false);
                PlayerSetup.Instance._movementSystem.ChangeProne(false);
                PlayerSetup.Instance.animatorManager.SetAnimatorParameterBool("Crouching", false);
                PlayerSetup.Instance.animatorManager.SetAnimatorParameterBool("Prone", false);
            }
        }
        public void SetProneLimit(float p_value)
        {
            if(!m_customProneLimit)
                m_proneLimit = Mathf.Clamp(p_value, 0f, 1f);
        }
    }
}

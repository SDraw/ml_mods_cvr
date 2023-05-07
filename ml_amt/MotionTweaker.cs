using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.IK.SubSystems;
using ABI_RC.Systems.MovementSystem;
using RootMotion.FinalIK;
using System.Collections.Generic;
using UnityEngine;

namespace ml_amt
{
    [DisallowMultipleComponent]
    class MotionTweaker : MonoBehaviour
    {
        static readonly Vector4 ms_pointVector = new Vector4(0f, 0f, 0f, 1f);
        static readonly int ms_emoteHash = Animator.StringToHash("Emote");

        enum PoseState
        {
            Standing = 0,
            Crouching,
            Proning
        }

        VRIK m_vrIk = null;
        int m_locomotionLayer = 0;
        float m_ikWeight = 1f; // Original weight
        float m_locomotionWeight = 1f; // Original weight
        bool m_plantFeet = false; // Original plant feet
        float m_avatarScale = 1f; // Instantiated scale
        Vector3 m_locomotionOffset = Vector3.zero; // Original locomotion offset
        bool m_bendNormalLeft = false;
        bool m_bendNormalRight = false;
        Transform m_avatarHips = null;
        float m_avatarHeight = 1f; // Initial avatar view height
        bool m_inVR = false;
        bool m_fbtAnimations = true;

        bool m_avatarReady = false;
        bool m_compatibleAvatar = false;
        float m_upright = 1f;
        PoseState m_poseState = PoseState.Standing;
        bool m_grounded = false;
        bool m_groundedRaw = false;
        bool m_moving = false;
        bool m_locomotionOverride = false;

        bool m_ikOverrideCrouch = true;
        float m_crouchLimit = 0.65f;
        bool m_customCrouchLimit = false;

        bool m_ikOverrideProne = true;
        float m_proneLimit = 0.3f;
        bool m_customProneLimit = false;

        bool m_poseTransitions = true;
        bool m_adjustedMovement = true;
        bool m_ikOverrideFly = true;
        bool m_ikOverrideJump = true;

        bool m_detectEmotes = true;
        bool m_emoteActive = false;

        bool m_followHips = true;
        Vector3 m_hipsToPlayer = Vector3.zero;

        Vector2 m_stepDistance = Vector2.zero;
        Vector3 m_massCenter = Vector3.zero;

        readonly List<AvatarParameter> m_parameters = null;

        internal MotionTweaker()
        {
            m_parameters = new List<AvatarParameter>();
        }

        // Unity events
        void Start()
        {
            m_inVR = Utils.IsInVR();

            Settings.IKOverrideCrouchChange += this.SetIKOverrideCrouch;
            Settings.CrouchLimitChange += this.SetCrouchLimit;
            Settings.IKOverrideProneChange += this.SetIKOverrideProne;
            Settings.ProneLimitChange += this.SetProneLimit;
            Settings.PoseTransitionsChange += this.SetPoseTransitions;
            Settings.AdjustedMovementChange += this.SetAdjustedMovement;
            Settings.IKOverrideFlyChange += this.SetIKOverrideFly;
            Settings.IKOverrideJumpChange += this.SetIKOverrideJump;
            Settings.DetectEmotesChange += this.SetDetectEmotes;
            Settings.FollowHipsChange += this.SetFollowHips;
            Settings.MassCenterChange += this.OnMassCenterChange;
            Settings.ScaledStepsChange += this.OnScaledStepsChange;

            m_fbtAnimations = MetaPort.Instance.settings.GetSettingsBool("GeneralEnableRunningAnimationFullBody");
            MetaPort.Instance.settings.settingBoolChanged.AddListener(this.OnGameSettingBoolChange);
        }

        void OnDestroy()
        {
            Settings.IKOverrideCrouchChange -= this.SetIKOverrideCrouch;
            Settings.CrouchLimitChange -= this.SetCrouchLimit;
            Settings.IKOverrideProneChange -= this.SetIKOverrideProne;
            Settings.ProneLimitChange -= this.SetProneLimit;
            Settings.PoseTransitionsChange -= this.SetPoseTransitions;
            Settings.AdjustedMovementChange -= this.SetAdjustedMovement;
            Settings.IKOverrideFlyChange -= this.SetIKOverrideFly;
            Settings.IKOverrideJumpChange -= this.SetIKOverrideJump;
            Settings.DetectEmotesChange -= this.SetDetectEmotes;
            Settings.FollowHipsChange -= this.SetFollowHips;
            Settings.MassCenterChange -= this.OnMassCenterChange;

            MetaPort.Instance.settings.settingBoolChanged.RemoveListener(this.OnGameSettingBoolChange);
        }

        void Update()
        {
            if(m_avatarReady)
            {
                m_grounded = MovementSystem.Instance.IsGrounded();
                m_groundedRaw = MovementSystem.Instance.IsGroundedRaw();
                m_moving = !Mathf.Approximately(MovementSystem.Instance.movementVector.magnitude, 0f);

                // Update upright
                Matrix4x4 l_hmdMatrix = PlayerSetup.Instance.transform.GetMatrix().inverse * PlayerSetup.Instance.GetActiveCamera().transform.GetMatrix();
                float l_currentHeight = Mathf.Clamp((l_hmdMatrix * ms_pointVector).y, 0f, float.MaxValue);
                float l_avatarViewHeight = Mathf.Clamp(m_avatarHeight * GetRelativeScale(), 0f, float.MaxValue);
                m_upright = Mathf.Clamp01((l_avatarViewHeight > 0f) ? (l_currentHeight / l_avatarViewHeight) : 0f);
                m_poseState = (m_upright <= Mathf.Min(m_proneLimit, m_crouchLimit)) ? PoseState.Proning : ((m_upright <= Mathf.Max(m_proneLimit, m_crouchLimit)) ? PoseState.Crouching : PoseState.Standing);

                if(m_avatarHips != null)
                {
                    Vector4 l_hipsToPoint = (PlayerSetup.Instance.transform.GetMatrix().inverse * m_avatarHips.GetMatrix()) * ms_pointVector;
                    m_hipsToPlayer.Set(l_hipsToPoint.x, 0f, l_hipsToPoint.z);
                }

                if(m_inVR && (m_vrIk != null) && m_vrIk.enabled)
                {
                    if(m_adjustedMovement)
                    {
                        MovementSystem.Instance.ChangeCrouch(m_poseState == PoseState.Crouching);
                        MovementSystem.Instance.ChangeProne(m_poseState == PoseState.Proning);

                        if(!m_poseTransitions)
                        {
                            PlayerSetup.Instance.animatorManager.SetAnimatorParameterBool("Crouching", false);
                            PlayerSetup.Instance.animatorManager.SetAnimatorParameterBool("Prone", false);
                        }
                    }

                    if(m_poseTransitions)
                    {
                        PlayerSetup.Instance.animatorManager.SetAnimatorParameterBool("Crouching", (m_poseState == PoseState.Crouching) && !m_compatibleAvatar && (!BodySystem.isCalibratedAsFullBody || m_fbtAnimations));
                        PlayerSetup.Instance.animatorManager.SetAnimatorParameterBool("Prone", (m_poseState == PoseState.Proning) && !m_compatibleAvatar && (!BodySystem.isCalibratedAsFullBody || m_fbtAnimations));
                    }
                }

                m_emoteActive = false;
                if(m_detectEmotes && (m_locomotionLayer >= 0))
                {
                    AnimatorStateInfo l_animState = PlayerSetup.Instance._animator.GetCurrentAnimatorStateInfo(m_locomotionLayer);
                    m_emoteActive = (l_animState.tagHash == ms_emoteHash);
                }

                if(m_parameters.Count > 0)
                {
                    foreach(AvatarParameter l_param in m_parameters)
                        l_param.Update(this);
                }
            }
        }

        // Game events
        internal void OnAvatarClear()
        {
            m_vrIk = null;
            m_locomotionLayer = -1;
            m_grounded = false;
            m_groundedRaw = false;
            m_avatarReady = false;
            m_compatibleAvatar = false;
            m_poseState = PoseState.Standing;
            m_customCrouchLimit = false;
            m_customProneLimit = false;
            m_avatarScale = 1f;
            m_locomotionOffset = Vector3.zero;
            m_emoteActive = false;
            m_moving = false;
            m_locomotionOverride = false;
            m_hipsToPlayer = Vector3.zero;
            m_avatarHips = null;
            m_avatarHeight = 1f;
            m_massCenter = Vector3.zero;
            m_stepDistance = Vector2.zero;
            m_parameters.Clear();
        }

        internal void OnSetupAvatar()
        {
            m_inVR = Utils.IsInVR();
            m_vrIk = PlayerSetup.Instance._avatar.GetComponent<VRIK>();
            m_locomotionLayer = PlayerSetup.Instance._animator.GetLayerIndex("Locomotion/Emotes");
            m_avatarHips = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Hips);
            m_avatarHeight = PlayerSetup.Instance._avatar.GetComponent<ABI.CCK.Components.CVRAvatar>().viewPosition.y;

            // Parse animator parameters
            m_parameters.Add(new AvatarParameter(AvatarParameter.ParameterType.Upright, PlayerSetup.Instance.animatorManager));
            m_parameters.Add(new AvatarParameter(AvatarParameter.ParameterType.GroundedRaw, PlayerSetup.Instance.animatorManager));
            m_parameters.Add(new AvatarParameter(AvatarParameter.ParameterType.Moving, PlayerSetup.Instance.animatorManager));
            m_parameters.RemoveAll(p => !p.IsValid());

            m_compatibleAvatar = m_parameters.Exists(p => (p.GetParameterType() == AvatarParameter.ParameterType.Upright));
            m_avatarScale = Mathf.Abs(PlayerSetup.Instance._avatar.transform.localScale.y);

            Transform l_customTransform = PlayerSetup.Instance._avatar.transform.Find("CrouchLimit");
            m_customCrouchLimit = (l_customTransform != null);
            m_crouchLimit = m_customCrouchLimit ? Mathf.Clamp01(l_customTransform.localPosition.y) : Settings.CrouchLimit;

            l_customTransform = PlayerSetup.Instance._avatar.transform.Find("ProneLimit");
            m_customProneLimit = (l_customTransform != null);
            m_proneLimit = m_customProneLimit ? Mathf.Clamp01(l_customTransform.localPosition.y) : Settings.ProneLimit;

            // Apply VRIK tweaks
            if(m_vrIk != null)
            {
                m_locomotionOffset = m_vrIk.solver.locomotion.offset;
                m_massCenter = m_locomotionOffset;

                if(m_vrIk.solver.HasToes())
                {
                    Transform l_foot = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                    if(l_foot == null)
                        l_foot = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightFoot);

                    Transform l_toe = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftToes);
                    if(l_toe == null)
                        l_toe = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightToes);

                    if((l_foot != null) && (l_toe != null))
                    {
                        Vector3 l_footPos = (PlayerSetup.Instance._avatar.transform.GetMatrix().inverse * l_foot.GetMatrix()) * ms_pointVector;
                        Vector3 l_toePos = (PlayerSetup.Instance._avatar.transform.GetMatrix().inverse * l_toe.GetMatrix()) * ms_pointVector;
                        m_massCenter = new Vector3(0f, 0f, l_toePos.z - l_footPos.z);
                    }
                }

                m_vrIk.solver.locomotion.offset = (Settings.MassCenter ? m_massCenter : m_locomotionOffset);

                m_stepDistance.Set(m_vrIk.solver.locomotion.stepThreshold, m_vrIk.solver.locomotion.footDistance);

                m_vrIk.onPreSolverUpdate.AddListener(this.OnIKPreUpdate);
                m_vrIk.onPostSolverUpdate.AddListener(this.OnIKPostUpdate);
            }

            m_avatarReady = true;
        }

        internal void OnCalibrate()
        {
            if(m_avatarReady && BodySystem.isCalibratedAsFullBody && BodySystem.enableHipTracking && !BodySystem.enableRightFootTracking && !BodySystem.enableLeftFootTracking && !BodySystem.enableLeftKneeTracking && !BodySystem.enableRightKneeTracking)
            {
                BodySystem.isCalibratedAsFullBody = false;
                BodySystem.TrackingLeftLegEnabled = false;
                BodySystem.TrackingRightLegEnabled = false;
                BodySystem.TrackingLocomotionEnabled = true;

                if(m_vrIk != null)
                    m_vrIk.solver.spine.maxRootAngle = 25f; // I need to rotate my legs, ffs!
            }
        }

        internal void OnPlayspaceScale()
        {
            if(m_vrIk != null)
            {
                if(Settings.MassCenter)
                    m_vrIk.solver.locomotion.offset = m_massCenter * GetRelativeScale();

                if(Settings.ScaledSteps)
                {
                    m_vrIk.solver.locomotion.stepThreshold = m_stepDistance.x * GetRelativeScale();
                    m_vrIk.solver.locomotion.footDistance = m_stepDistance.y * GetRelativeScale();

                    m_vrIk.solver.locomotion.stepHeight.keys = Utils.GetSineKeyframes(Mathf.Clamp01(PlayerSetup.Instance.GetAvatarHeight()) * 0.03f);
                    m_vrIk.solver.locomotion.heelHeight.keys = Utils.GetSineKeyframes(Mathf.Clamp01(PlayerSetup.Instance.GetAvatarHeight()) * 0.03f);
                }
            }
        }

        // IK events
        void OnIKPreUpdate()
        {
            bool l_locomotionOverride = false;

            m_ikWeight = m_vrIk.solver.IKPositionWeight;
            m_locomotionWeight = m_vrIk.solver.locomotion.weight;
            m_plantFeet = m_vrIk.solver.plantFeet;
            m_bendNormalLeft = m_vrIk.solver.leftLeg.useAnimatedBendNormal;
            m_bendNormalRight = m_vrIk.solver.rightLeg.useAnimatedBendNormal;

            if(m_detectEmotes && m_emoteActive)
                m_vrIk.solver.IKPositionWeight = 0f;

            if(!BodySystem.isCalibratedAsFullBody)
            {
                if((m_ikOverrideCrouch && (m_poseState != PoseState.Standing)) || (m_ikOverrideProne && (m_poseState == PoseState.Proning)))
                {
                    m_vrIk.solver.locomotion.weight = 0f;
                    m_vrIk.solver.leftLeg.useAnimatedBendNormal = true;
                    m_vrIk.solver.rightLeg.useAnimatedBendNormal = true;
                    l_locomotionOverride = true;
                }
                if(m_ikOverrideFly && MovementSystem.Instance.flying)
                {
                    m_vrIk.solver.locomotion.weight = 0f;
                    m_vrIk.solver.leftLeg.useAnimatedBendNormal = true;
                    m_vrIk.solver.rightLeg.useAnimatedBendNormal = true;
                    l_locomotionOverride = true;
                }
                if(m_ikOverrideJump && !m_grounded && !MovementSystem.Instance.flying)
                {
                    m_vrIk.solver.locomotion.weight = 0f;
                    m_vrIk.solver.leftLeg.useAnimatedBendNormal = true;
                    m_vrIk.solver.rightLeg.useAnimatedBendNormal = true;
                    l_locomotionOverride = true;
                }
            }

            bool l_solverActive = !Mathf.Approximately(m_vrIk.solver.IKPositionWeight, 0f);
            if(l_locomotionOverride && l_solverActive && m_followHips && (!m_moving || (m_poseState == PoseState.Proning)) && m_inVR && !BodySystem.isCalibratedAsFullBody && !ModSupporter.SkipHipsOverride())
            {
                m_vrIk.solver.plantFeet = false;
                IKSystem.VrikRootController.enabled = false;
                PlayerSetup.Instance._avatar.transform.localPosition = m_hipsToPlayer;
            }

            if(m_locomotionOverride && !l_locomotionOverride)
                m_vrIk.solver.Reset();
            m_locomotionOverride = l_locomotionOverride;
        }

        void OnIKPostUpdate()
        {
            m_vrIk.solver.IKPositionWeight = m_ikWeight;
            m_vrIk.solver.locomotion.weight = m_locomotionWeight;
            m_vrIk.solver.plantFeet = m_plantFeet;
            m_vrIk.solver.leftLeg.useAnimatedBendNormal = m_bendNormalLeft;
            m_vrIk.solver.rightLeg.useAnimatedBendNormal = m_bendNormalRight;
        }

        // Settings
        internal void SetIKOverrideCrouch(bool p_state)
        {
            m_ikOverrideCrouch = p_state;
        }
        internal void SetCrouchLimit(float p_value)
        {
            if(!m_customCrouchLimit)
                m_crouchLimit = Mathf.Clamp01(p_value);
        }
        internal void SetIKOverrideProne(bool p_state)
        {
            m_ikOverrideProne = p_state;
        }
        internal void SetProneLimit(float p_value)
        {
            if(!m_customProneLimit)
                m_proneLimit = Mathf.Clamp01(p_value);
        }
        internal void SetPoseTransitions(bool p_state)
        {
            m_poseTransitions = p_state;

            if(!m_poseTransitions && m_avatarReady && m_inVR)
            {
                PlayerSetup.Instance.animatorManager.SetAnimatorParameterBool("Crouching", false);
                PlayerSetup.Instance.animatorManager.SetAnimatorParameterBool("Prone", false);
            }
        }
        internal void SetAdjustedMovement(bool p_state)
        {
            m_adjustedMovement = p_state;

            if(!m_adjustedMovement && m_avatarReady && m_inVR)
            {
                MovementSystem.Instance.ChangeCrouch(false);
                MovementSystem.Instance.ChangeProne(false);
            }
        }
        internal void SetIKOverrideFly(bool p_state)
        {
            m_ikOverrideFly = p_state;
        }
        internal void SetIKOverrideJump(bool p_state)
        {
            m_ikOverrideJump = p_state;
        }
        internal void SetDetectEmotes(bool p_state)
        {
            m_detectEmotes = p_state;
        }
        internal void SetFollowHips(bool p_state)
        {
            m_followHips = p_state;
        }
        void OnMassCenterChange(bool p_state)
        {
            if(m_vrIk != null)
                m_vrIk.solver.locomotion.offset = (Settings.MassCenter ? (m_massCenter * GetRelativeScale()) : m_locomotionOffset);
        }
        void OnScaledStepsChange(bool p_state)
        {
            if(m_vrIk != null)
            {
                if(p_state)
                {
                    m_vrIk.solver.locomotion.stepThreshold = m_stepDistance.x * GetRelativeScale();
                    m_vrIk.solver.locomotion.footDistance = m_stepDistance.y * GetRelativeScale();
                    m_vrIk.solver.locomotion.stepHeight.keys = Utils.GetSineKeyframes(Mathf.Clamp01(PlayerSetup.Instance.GetAvatarHeight()) * 0.03f);
                    m_vrIk.solver.locomotion.heelHeight.keys = Utils.GetSineKeyframes(Mathf.Clamp01(PlayerSetup.Instance.GetAvatarHeight()) * 0.03f);
                }
                else
                {
                    IKSystem.Instance.ApplyAvatarScaleToIk(PlayerSetup.Instance.GetAvatarHeight());
                    m_vrIk.solver.locomotion.stepHeight.keys = Utils.GetSineKeyframes(0.03f);
                    m_vrIk.solver.locomotion.heelHeight.keys = Utils.GetSineKeyframes(0.03f);
                }
            }
        }

        // Game settings
        void OnGameSettingBoolChange(string p_name, bool p_state)
        {
            if(p_name == "GeneralEnableRunningAnimationFullBody")
                m_fbtAnimations = p_state;
        }

        // Arbitrary
        float GetRelativeScale()
        {
            return ((m_avatarScale > 0f) ? (PlayerSetup.Instance._avatar.transform.localScale.y / m_avatarScale) : 0f);
        }

        // Parameters access
        public float GetUpright() => m_upright;
        public bool GetGroundedRaw() => m_groundedRaw;
        public bool GetMoving() => m_moving;
    }
}

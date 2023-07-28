using ABI_RC.Core.Player;
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
        struct IKState
        {
            public float m_weight;
            public float m_locomotionWeight;
            public bool m_plantFeet;
            public bool m_bendNormalLeft;
            public bool m_bendNormalRight;
        }

        static readonly Vector4 ms_pointVector = new Vector4(0f, 0f, 0f, 1f);
        static readonly int ms_emoteHash = Animator.StringToHash("Emote");

        IKState m_ikState;
        VRIK m_vrIk = null;
        int m_locomotionLayer = 0;
        float m_avatarScale = 1f;
        Vector3 m_locomotionOffset = Vector3.zero; // Original locomotion offset
        Transform m_avatarHips = null;
        bool m_inVR = false;

        bool m_avatarReady = false;
        bool m_grounded = false;
        bool m_groundedRaw = false;
        bool m_moving = false;
        bool m_locomotionOverride = false;

        bool m_ikOverrideFly = true;
        bool m_ikOverrideJump = true;

        bool m_detectEmotes = true;
        bool m_emoteActive = false;

        bool m_followHips = true;
        Vector3 m_hipsToPlayer = Vector3.zero;

        Vector3 m_massCenter = Vector3.zero;

        Transform m_ikLimits = null;

        readonly List<AvatarParameter> m_parameters = null;

        internal MotionTweaker()
        {
            m_parameters = new List<AvatarParameter>();
        }

        // Unity events
        void Start()
        {
            m_inVR = Utils.IsInVR();

            Settings.CrouchLimitChange += this.SetCrouchLimit;
            Settings.ProneLimitChange += this.SetProneLimit;
            Settings.IKOverrideFlyChange += this.SetIKOverrideFly;
            Settings.IKOverrideJumpChange += this.SetIKOverrideJump;
            Settings.DetectEmotesChange += this.SetDetectEmotes;
            Settings.FollowHipsChange += this.SetFollowHips;
            Settings.MassCenterChange += this.OnMassCenterChange;

            SetCrouchLimit(Settings.CrouchLimit);
            SetProneLimit(Settings.ProneLimit);
        }

        void OnDestroy()
        {
            Settings.CrouchLimitChange -= this.SetCrouchLimit;
            Settings.ProneLimitChange -= this.SetProneLimit;
            Settings.IKOverrideFlyChange -= this.SetIKOverrideFly;
            Settings.IKOverrideJumpChange -= this.SetIKOverrideJump;
            Settings.DetectEmotesChange -= this.SetDetectEmotes;
            Settings.FollowHipsChange -= this.SetFollowHips;
            Settings.MassCenterChange -= this.OnMassCenterChange;
        }

        void Update()
        {
            if(m_avatarReady)
            {
                m_grounded = MovementSystem.Instance.IsGrounded();
                m_groundedRaw = MovementSystem.Instance.IsGroundedRaw();
                m_moving = !Mathf.Approximately(MovementSystem.Instance.movementVector.magnitude, 0f);

                UpdateIKLimits();

                if(m_avatarHips != null)
                {
                    Vector4 l_hipsToPoint = (PlayerSetup.Instance.transform.GetMatrix().inverse * m_avatarHips.GetMatrix()) * ms_pointVector;
                    m_hipsToPlayer.Set(l_hipsToPoint.x, 0f, l_hipsToPoint.z);
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
            m_avatarScale = 1f;
            m_locomotionOffset = Vector3.zero;
            m_emoteActive = false;
            m_moving = false;
            m_locomotionOverride = false;
            m_hipsToPlayer = Vector3.zero;
            m_avatarHips = null;
            m_massCenter = Vector3.zero;
            m_ikLimits = null;
            m_parameters.Clear();

            PlayerSetup.Instance.avatarCrouchLimit = Mathf.Max(Mathf.Clamp01(Settings.CrouchLimit), Mathf.Clamp01(Settings.CrouchLimit));
            PlayerSetup.Instance.avatarProneLimit = Mathf.Min(Mathf.Clamp01(Settings.CrouchLimit), Mathf.Clamp01(Settings.CrouchLimit));
        }

        internal void OnSetupAvatar()
        {
            m_inVR = Utils.IsInVR();
            m_vrIk = PlayerSetup.Instance._avatar.GetComponent<VRIK>();
            m_locomotionLayer = PlayerSetup.Instance._animator.GetLayerIndex("Locomotion/Emotes");
            m_avatarHips = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Hips);
            m_avatarScale = Mathf.Abs(PlayerSetup.Instance._avatar.transform.localScale.y);

            // Parse animator parameters
            m_parameters.Add(new AvatarParameter(AvatarParameter.ParameterType.Upright, PlayerSetup.Instance.animatorManager));
            m_parameters.Add(new AvatarParameter(AvatarParameter.ParameterType.GroundedRaw, PlayerSetup.Instance.animatorManager));
            m_parameters.Add(new AvatarParameter(AvatarParameter.ParameterType.Moving, PlayerSetup.Instance.animatorManager));
            m_parameters.RemoveAll(p => !p.IsValid());

            // Avatar custom IK limits
            m_ikLimits = PlayerSetup.Instance._avatar.transform.Find("[IKLimits]");
            UpdateIKLimits();

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

                m_vrIk.onPreSolverUpdate.AddListener(this.OnIKPreUpdate);
                m_vrIk.onPostSolverUpdate.AddListener(this.OnIKPostUpdate);
            }

            m_avatarReady = true;
        }

        internal void OnCalibrate()
        {
            if(m_avatarReady && (m_vrIk != null) && (m_vrIk.solver.spine.pelvisTarget != null) && (m_vrIk.solver.leftLeg.target == null) && (m_vrIk.solver.rightLeg.target == null))
            {
                // Do not consider 4PT as FBT (!!!)
                m_vrIk.solver.spine.bodyPosStiffness = 0.55f;
                m_vrIk.solver.spine.bodyRotStiffness = 0.1f;
                m_vrIk.solver.spine.neckStiffness = 0.5f;
                m_vrIk.solver.spine.chestClampWeight = 0.55f;
                m_vrIk.solver.spine.moveBodyBackWhenCrouching = 0.5f;
                m_vrIk.solver.spine.maxRootAngle = 25f;
                m_vrIk.fixTransforms = false;

                BodySystem.isCalibratedAsFullBody = false;
                BodySystem.TrackingLeftLegEnabled = false;
                BodySystem.TrackingRightLegEnabled = false;
                BodySystem.TrackingLocomotionEnabled = true;

                IKSystem.Instance.applyOriginalHipRotation = true;
            }
        }

        internal void OnPlayspaceScale()
        {
            if((m_vrIk != null) && Settings.MassCenter)
                m_vrIk.solver.locomotion.offset = m_massCenter * GetRelativeScale();
        }

        // IK events
        void OnIKPreUpdate()
        {
            bool l_locomotionOverride = false;

            m_ikState.m_weight = m_vrIk.solver.IKPositionWeight;
            m_ikState.m_locomotionWeight = m_vrIk.solver.locomotion.weight;
            m_ikState.m_plantFeet = m_vrIk.solver.plantFeet;
            m_ikState.m_bendNormalLeft = m_vrIk.solver.leftLeg.useAnimatedBendNormal;
            m_ikState.m_bendNormalRight = m_vrIk.solver.rightLeg.useAnimatedBendNormal;

            if(m_detectEmotes && m_emoteActive)
                m_vrIk.solver.IKPositionWeight = 0f;

            if(!BodySystem.isCalibratedAsFullBody)
            {
                if(PlayerSetup.Instance.avatarUpright <= PlayerSetup.Instance.avatarCrouchLimit)
                {
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
            if(l_locomotionOverride && l_solverActive && m_followHips && (!m_moving || (PlayerSetup.Instance.avatarUpright <= PlayerSetup.Instance.avatarCrouchLimit)) && m_inVR && !BodySystem.isCalibratedAsFullBody && !ModSupporter.SkipHipsOverride())
            {
                m_vrIk.solver.plantFeet = false;
                if(IKSystem.VrikRootController != null)
                    IKSystem.VrikRootController.enabled = false;
                PlayerSetup.Instance._avatar.transform.localPosition = m_hipsToPlayer;
            }

            if(m_locomotionOverride && !l_locomotionOverride)
            {
                m_vrIk.solver.Reset();
                if(IKSystem.VrikRootController != null)
                    IKSystem.VrikRootController.enabled = true;
            }
            m_locomotionOverride = l_locomotionOverride;
        }

        void OnIKPostUpdate()
        {
            m_vrIk.solver.IKPositionWeight = m_ikState.m_weight;
            m_vrIk.solver.locomotion.weight = m_ikState.m_locomotionWeight;
            m_vrIk.solver.plantFeet = m_ikState.m_plantFeet;
            m_vrIk.solver.leftLeg.useAnimatedBendNormal = m_ikState.m_bendNormalLeft;
            m_vrIk.solver.rightLeg.useAnimatedBendNormal = m_ikState.m_bendNormalRight;
        }

        // Settings
        internal void SetCrouchLimit(float p_value)
        {
            if(m_ikLimits == null)
                PlayerSetup.Instance.avatarCrouchLimit = Mathf.Max(Mathf.Clamp01(p_value), PlayerSetup.Instance.avatarProneLimit);
        }
        internal void SetProneLimit(float p_value)
        {
            if(m_ikLimits == null)
                PlayerSetup.Instance.avatarProneLimit = Mathf.Min(Mathf.Clamp01(p_value), PlayerSetup.Instance.avatarCrouchLimit);
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

        // Arbitrary
        float GetRelativeScale()
        {
            return ((m_avatarScale > 0f) ? (PlayerSetup.Instance._avatar.transform.localScale.y / m_avatarScale) : 0f);
        }

        void UpdateIKLimits()
        {
            if(m_ikLimits != null)
            {
                Vector3 l_values = m_ikLimits.localPosition;
                PlayerSetup.Instance.avatarCrouchLimit = Mathf.Max(Mathf.Clamp01(l_values.x), Mathf.Clamp01(l_values.y));
                PlayerSetup.Instance.avatarProneLimit = Mathf.Min(Mathf.Clamp01(l_values.x), Mathf.Clamp01(l_values.y));
            }
        }

        // Parameters access
        public float GetUpright() => PlayerSetup.Instance.avatarUpright;
        public bool GetGroundedRaw() => m_groundedRaw;
        public bool GetMoving() => m_moving;
    }
}

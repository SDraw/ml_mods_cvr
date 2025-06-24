using ABI.CCK.Components;
using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Player;
using ABI_RC.Systems.GameEventSystem;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.IK.SubSystems;
using ABI_RC.Systems.InputManagement;
using ABI_RC.Systems.Movement;
using RootMotion.FinalIK;
using UnityEngine;

namespace ml_pmc
{
    [DisallowMultipleComponent]
    class PoseCopycat : MonoBehaviour
    {
        public class CopycatEvent<T1>
        {
            event System.Action<T1> m_action;
            public void AddListener(System.Action<T1> p_listener) => m_action += p_listener;
            public void RemoveListener(System.Action<T1> p_listener) => m_action -= p_listener;
            public void Invoke(T1 p_value) => m_action?.Invoke(p_value);
        }

        static readonly Vector4 ms_pointVector = new Vector4(0f, 0f, 0f, 1f);

        static PoseCopycat ms_instance = null;
        internal static readonly CopycatEvent<bool> OnCopycatChanged = new CopycatEvent<bool>();

        Animator m_animator = null;
        VRIK m_vrIk = null;
        float m_ikWeight = 1f;
        LookAtIK m_lookAtIk = null;
        float m_lookIkWeight = 1f;
        bool m_sitting = false;
        bool m_inVr = false;

        bool m_active = false;
        float m_distanceLimit = float.MaxValue;
        bool m_fingerTracking = false;

        HumanPoseHandler m_poseHandler = null;
        HumanPose m_pose;
        PuppetParser m_puppetParser = null;

        void Awake()
        {
            if(ms_instance != null)
            {
                DestroyImmediate(this);
                return;
            }

            ms_instance = this;
            DontDestroyOnLoad(this);
        }

        void Start()
        {
            CVRGameEventSystem.Avatar.OnLocalAvatarClear.AddListener(this.OnAvatarClear);
            CVRGameEventSystem.Avatar.OnLocalAvatarLoad.AddListener(this.OnAvatarSetup);
            GameEvents.OnAvatarPreReuse.AddListener(this.OnAvatarPreReuse);
            GameEvents.OnAvatarPostReuse.AddListener(this.OnAvatarPostReuse);

            ModUi.OnTargetSelect.AddListener(this.OnTargetSelect);
        }
        void OnDestroy()
        {
            if(ms_instance == this)
                ms_instance = null;

            m_poseHandler?.Dispose();
            m_poseHandler = null;

            if(m_puppetParser != null)
                Object.Destroy(m_puppetParser);
            m_puppetParser = null;

            m_animator = null;
            m_vrIk = null;
            m_lookAtIk = null;

            CVRGameEventSystem.Avatar.OnLocalAvatarClear.RemoveListener(this.OnAvatarClear);
            CVRGameEventSystem.Avatar.OnLocalAvatarLoad.RemoveListener(this.OnAvatarSetup);
            GameEvents.OnAvatarPreReuse.RemoveListener(this.OnAvatarPreReuse);
            GameEvents.OnAvatarPostReuse.RemoveListener(this.OnAvatarPostReuse);

            ModUi.OnTargetSelect.RemoveListener(this.OnTargetSelect);
        }

        // Unity events
        void Update()
        {
            m_sitting = BetterBetterCharacterController.Instance.IsSitting();

            if(m_active)
            {
                if(m_puppetParser != null)
                {
                    OverrideIK();

                    bool l_mirror = Settings.MirrorPose;

                    if(Settings.Gestures)
                    {
                        CVRInputManager.Instance.gestureLeft = (l_mirror ? m_puppetParser.GetRightGesture() : m_puppetParser.GetLeftGesture());
                        CVRInputManager.Instance.gestureRight = (l_mirror ? m_puppetParser.GetLeftGesture() : m_puppetParser.GetRightGesture());
                    }

                    if(m_puppetParser.HasFingerTracking())
                    {
                        m_fingerTracking = true;

                        CVRInputManager.Instance.individualFingerTracking = true;
                        IKSystem.Instance.FingerSystem.controlActive = true;

                        ref readonly float[] l_curls = ref m_puppetParser.GetFingerCurls();
                        ref readonly float[] l_spreads = ref m_puppetParser.GetFingerSpreads();

                        // Left hand
                        CVRInputManager.Instance.finger1StretchedLeftThumb = l_curls[l_mirror ? 15 : 0];
                        CVRInputManager.Instance.finger2StretchedLeftThumb = l_curls[l_mirror ? 16 : 1];
                        CVRInputManager.Instance.finger3StretchedLeftThumb = l_curls[l_mirror ? 17 : 2];
                        CVRInputManager.Instance.fingerSpreadLeftThumb = l_spreads[l_mirror ? 5 : 0];

                        CVRInputManager.Instance.finger1StretchedLeftIndex = l_curls[l_mirror ? 18 : 3];
                        CVRInputManager.Instance.finger2StretchedLeftIndex = l_curls[l_mirror ? 19 : 4];
                        CVRInputManager.Instance.finger3StretchedLeftIndex = l_curls[l_mirror ? 20 : 5];
                        CVRInputManager.Instance.fingerSpreadLeftIndex = l_spreads[l_mirror ? 6 : 1];

                        CVRInputManager.Instance.finger1StretchedLeftMiddle = l_curls[l_mirror ? 21 : 6];
                        CVRInputManager.Instance.finger2StretchedLeftMiddle = l_curls[l_mirror ? 22 : 7];
                        CVRInputManager.Instance.finger3StretchedLeftMiddle = l_curls[l_mirror ? 23 : 8];
                        CVRInputManager.Instance.fingerSpreadLeftMiddle = l_spreads[l_mirror ? 7 : 2];

                        CVRInputManager.Instance.finger1StretchedLeftRing = l_curls[l_mirror ? 24 : 9];
                        CVRInputManager.Instance.finger2StretchedLeftRing = l_curls[l_mirror ? 25 : 10];
                        CVRInputManager.Instance.finger3StretchedLeftRing = l_curls[l_mirror ? 26 : 11];
                        CVRInputManager.Instance.fingerSpreadLeftRing = l_spreads[l_mirror ? 8 : 3];

                        CVRInputManager.Instance.finger1StretchedLeftPinky = l_curls[l_mirror ? 27 : 12];
                        CVRInputManager.Instance.finger2StretchedLeftPinky = l_curls[l_mirror ? 28 : 13];
                        CVRInputManager.Instance.finger3StretchedLeftPinky = l_curls[l_mirror ? 29 : 14];
                        CVRInputManager.Instance.fingerSpreadLeftPinky = l_spreads[l_mirror ? 9 : 4];

                        // Right hand
                        CVRInputManager.Instance.finger1StretchedRightThumb = l_curls[l_mirror ? 0 : 15];
                        CVRInputManager.Instance.finger2StretchedRightThumb = l_curls[l_mirror ? 1 : 16];
                        CVRInputManager.Instance.finger3StretchedRightThumb = l_curls[l_mirror ? 2 : 17];
                        CVRInputManager.Instance.fingerSpreadRightThumb = l_spreads[l_mirror ? 0 : 5];

                        CVRInputManager.Instance.finger1StretchedRightIndex = l_curls[l_mirror ? 3 : 18];
                        CVRInputManager.Instance.finger2StretchedRightIndex = l_curls[l_mirror ? 4 : 19];
                        CVRInputManager.Instance.finger3StretchedRightIndex = l_curls[l_mirror ? 5 : 20];
                        CVRInputManager.Instance.fingerSpreadRightIndex = l_spreads[l_mirror ? 1 : 6];

                        CVRInputManager.Instance.finger1StretchedRightMiddle = l_curls[l_mirror ? 6 : 21];
                        CVRInputManager.Instance.finger2StretchedRightMiddle = l_curls[l_mirror ? 7 : 22];
                        CVRInputManager.Instance.finger3StretchedRightMiddle = l_curls[l_mirror ? 8 : 23];
                        CVRInputManager.Instance.fingerSpreadRightMiddle = l_spreads[l_mirror ? 2 : 7];

                        CVRInputManager.Instance.finger1StretchedRightRing = l_curls[l_mirror ? 9 : 24];
                        CVRInputManager.Instance.finger2StretchedRightRing = l_curls[l_mirror ? 10 : 25];
                        CVRInputManager.Instance.finger3StretchedRightRing = l_curls[l_mirror ? 11 : 26];
                        CVRInputManager.Instance.fingerSpreadRightRing = l_spreads[l_mirror ? 3 : 8];

                        CVRInputManager.Instance.finger1StretchedRightPinky = l_curls[l_mirror ? 12 : 27];
                        CVRInputManager.Instance.finger2StretchedRightPinky = l_curls[l_mirror ? 13 : 28];
                        CVRInputManager.Instance.finger3StretchedRightPinky = l_curls[l_mirror ? 14 : 29];
                        CVRInputManager.Instance.fingerSpreadRightPinky = l_spreads[l_mirror ? 4 : 9];
                    }
                    else
                    {
                        if(m_fingerTracking)
                        {
                            RestoreFingerTracking();
                            m_fingerTracking = false;
                        }
                    }

                    Matrix4x4 l_offset = m_puppetParser.GetLastOffset();
                    Vector3 l_pos = l_offset * ms_pointVector;
                    Quaternion l_rot = l_offset.rotation;

                    l_pos.y = 0f;
                    if(Settings.MirrorPosition)
                        l_pos.x *= -1f;
                    l_pos = Vector3.ClampMagnitude(l_pos, m_distanceLimit);

                    l_rot = Quaternion.Euler(0f, l_rot.eulerAngles.y * (Settings.MirrorRotation ? -1f : 1f), 0f);

                    Matrix4x4 l_result = PlayerSetup.Instance.transform.GetMatrix() * Matrix4x4.TRS(l_pos, l_rot, Vector3.one);

                    if(Settings.Position && !m_sitting && !m_puppetParser.IsSitting() && Utils.IsWorldSafe() && Utils.IsCombatSafe())
                        PlayerSetup.Instance.transform.position = l_result * ms_pointVector;

                    if(Settings.Rotation && !m_sitting && !m_puppetParser.IsSitting() && Utils.IsCombatSafe())
                    {
                        if(m_inVr)
                        {
                            Vector3 l_avatarPos = PlayerSetup.Instance.AvatarTransform.position;
                            PlayerSetup.Instance.transform.rotation = l_result.rotation;
                            Vector3 l_dif = l_avatarPos - PlayerSetup.Instance.AvatarTransform.position;
                            PlayerSetup.Instance.transform.position += l_dif;
                        }
                        else
                            PlayerSetup.Instance.transform.rotation = l_result.rotation;
                    }

                    if(Vector3.Distance(PlayerSetup.Instance.GetPlayerPosition(), m_puppetParser.transform.position) > m_distanceLimit)
                        SetTarget(null);
                }
                else
                    SetTarget(null);
            }
        }

        void LateUpdate()
        {
            if(m_active && (m_animator != null) && (m_puppetParser != null))
            {
                OverrideIK();

                m_puppetParser.GetPose().CopyTo(ref m_pose);

                if(Settings.MirrorPose)
                    Utils.MirrorPose(ref m_pose);

                m_poseHandler.SetHumanPose(ref m_pose);
            }
        }

        // Game events
        void OnAvatarClear(CVRAvatar p_avatar)
        {
            try
            {
                if(m_active)
                {
                    RestoreIK();
                    RestoreFingerTracking();
                    OnCopycatChanged.Invoke(false);
                }
                m_active = false;

                if(m_puppetParser != null)
                    Object.Destroy(m_puppetParser);
                m_puppetParser = null;

                m_animator = null;
                m_vrIk = null;
                m_lookAtIk = null;

                m_poseHandler?.Dispose();
                m_poseHandler = null;

                m_distanceLimit = float.MaxValue;
                m_fingerTracking = false;
                m_pose = new HumanPose();
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        void OnAvatarSetup(CVRAvatar p_avatar)
        {
            try
            {
                m_inVr = Utils.IsInVR();
                m_animator = PlayerSetup.Instance.Animator;
                m_vrIk = PlayerSetup.Instance.AvatarObject.GetComponent<VRIK>();
                m_lookAtIk = PlayerSetup.Instance.AvatarObject.GetComponent<LookAtIK>();

                if((m_animator != null) && m_animator.isHuman)
                {
                    m_poseHandler = new HumanPoseHandler(m_animator.avatar, m_animator.transform);
                    m_poseHandler.GetHumanPose(ref m_pose);

                    if(m_vrIk != null)
                    {
                        m_vrIk.onPreSolverUpdate.AddListener(this.OnVRIKPreUpdate);
                        m_vrIk.onPostSolverUpdate.AddListener(this.OnVRIKPostUpdate);
                    }

                    if(m_lookAtIk != null)
                    {
                        m_lookAtIk.onPreSolverUpdate.AddListener(this.OnLookAtIKPreUpdate);
                        m_lookAtIk.onPostSolverUpdate.AddListener(this.OnLookAtIKPostUpdate);
                    }
                }
                else
                    m_animator = null;
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        void OnAvatarPreReuse()
        {
            if(m_active)
                SetTarget(null);
        }
        void OnAvatarPostReuse()
        {
            m_inVr = Utils.IsInVR();

            // Old VRIK and LookAtIK are destroyed by game
            m_vrIk = PlayerSetup.Instance.AvatarObject.GetComponent<VRIK>();
            m_lookAtIk = PlayerSetup.Instance.AvatarObject.GetComponent<LookAtIK>();

            if(m_vrIk != null)
            {
                m_vrIk.onPreSolverUpdate.AddListener(this.OnVRIKPreUpdate);
                m_vrIk.onPostSolverUpdate.AddListener(this.OnVRIKPostUpdate);
            }

            if(m_lookAtIk != null)
            {
                m_lookAtIk.onPreSolverUpdate.AddListener(this.OnLookAtIKPreUpdate);
                m_lookAtIk.onPostSolverUpdate.AddListener(this.OnLookAtIKPostUpdate);
            }
        }

        // Ui events
        void OnTargetSelect(string p_id)
        {
            if(m_active)
                SetTarget(null);
            else
            {
                if(m_animator != null)
                {
                    if(Friends.FriendsWith(p_id))
                    {
                        if(CVRPlayerManager.Instance.UserIdToPlayerEntity.TryGetValue(p_id, out CVRPlayerEntity l_playerEntity))
                        {
                            if(Utils.IsInSight(BetterBetterCharacterController.Instance.KinematicTriggerProxy.Collider, l_playerEntity.PuppetMaster.GetComponent<CapsuleCollider>(), Utils.GetWorldMovementLimit()))
                                SetTarget(l_playerEntity.PuppetMaster);
                            else
                                ModUi.ShowAlert("Selected player is too far away or obstructed");
                        }
                        else
                            ModUi.ShowAlert("Selected player isn't connected or ready yet");
                    }
                    else
                        ModUi.ShowAlert("Selected player isn't your friend");
                }
                else
                    ModUi.ShowAlert("Local avatar isn't ready yet");
            }
        }

        // IK updates
        void OnVRIKPreUpdate()
        {
            if(m_active)
            {
                m_ikWeight = m_vrIk.solver.IKPositionWeight;
                m_vrIk.solver.IKPositionWeight = 0f;
            }
        }
        void OnVRIKPostUpdate()
        {
            if(m_active)
                m_vrIk.solver.IKPositionWeight = m_ikWeight;
        }

        void OnLookAtIKPreUpdate()
        {
            if(m_active && !Settings.LookAtMix)
            {
                m_lookIkWeight = m_lookAtIk.solver.IKPositionWeight;
                m_lookAtIk.solver.IKPositionWeight = 0f;
            }
        }
        void OnLookAtIKPostUpdate()
        {
            if(m_active && !Settings.LookAtMix)
                m_lookAtIk.solver.IKPositionWeight = m_lookIkWeight;
        }

        // Arbitrary
        void SetTarget(PuppetMaster p_target)
        {
            if(m_animator != null)
            {
                if(!m_active)
                {
                    if((p_target != null) && (p_target.AnimatorManager != null) && (p_target.Animator != null) && p_target.Animator.isHuman)
                    {
                        m_puppetParser = p_target.Animator.gameObject.AddComponent<PuppetParser>();
                        m_puppetParser.m_puppetMaster = p_target;
                        m_distanceLimit = Utils.GetWorldMovementLimit();

                        m_active = true;
                        OnCopycatChanged.Invoke(m_active);
                    }
                }
                else
                {
                    if(p_target == null)
                    {
                        if(m_puppetParser != null)
                            Object.Destroy(m_puppetParser);
                        m_puppetParser = null;

                        if(!m_sitting)
                        {
                            Quaternion l_rot = PlayerSetup.Instance.transform.rotation;
                            PlayerSetup.Instance.transform.rotation = Quaternion.Euler(0f, l_rot.eulerAngles.y, 0f);
                        }

                        RestoreIK();
                        RestoreFingerTracking();
                        m_fingerTracking = false;

                        m_active = false;
                        OnCopycatChanged.Invoke(m_active);
                    }
                }
            }
        }

        void OverrideIK()
        {
            if(!BodySystem.isCalibrating)
                BodySystem.TrackingPositionWeight = 0f;
        }
        void RestoreIK()
        {
            if(!BodySystem.isCalibrating)
                BodySystem.TrackingPositionWeight = 1f;

            if(m_vrIk != null)
                m_vrIk.solver.Reset();
        }
        void RestoreFingerTracking()
        {
            CVRInputManager.Instance.individualFingerTracking = (m_inVr && Utils.AreKnucklesInUse() && !CVRInputManager._moduleXR.SkeletalToggleValue);
            IKSystem.Instance.FingerSystem.controlActive = CVRInputManager.Instance.individualFingerTracking;

            if(!CVRInputManager.Instance.individualFingerTracking)
            {
                // Left hand
                CVRInputManager.Instance.finger1StretchedLeftThumb = 0f;
                CVRInputManager.Instance.finger2StretchedLeftThumb = 0f;
                CVRInputManager.Instance.finger3StretchedLeftThumb = 0f;
                CVRInputManager.Instance.fingerSpreadLeftThumb = 0f;

                CVRInputManager.Instance.finger1StretchedLeftIndex = 0f;
                CVRInputManager.Instance.finger2StretchedLeftIndex = 0f;
                CVRInputManager.Instance.finger3StretchedLeftIndex = 0f;
                CVRInputManager.Instance.fingerSpreadLeftIndex = 0f;

                CVRInputManager.Instance.finger1StretchedLeftMiddle = 0;
                CVRInputManager.Instance.finger2StretchedLeftMiddle = 0f;
                CVRInputManager.Instance.finger3StretchedLeftMiddle = 0f;
                CVRInputManager.Instance.fingerSpreadLeftMiddle = 0f;

                CVRInputManager.Instance.finger1StretchedLeftRing = 0f;
                CVRInputManager.Instance.finger2StretchedLeftRing = 0f;
                CVRInputManager.Instance.finger3StretchedLeftRing = 0f;
                CVRInputManager.Instance.fingerSpreadLeftRing = 0f;

                CVRInputManager.Instance.finger1StretchedLeftPinky = 0f;
                CVRInputManager.Instance.finger2StretchedLeftPinky = 0f;
                CVRInputManager.Instance.finger3StretchedLeftPinky = 0f;
                CVRInputManager.Instance.fingerSpreadLeftPinky = 0f;

                CVRInputManager.Instance.fingerFullCurlNormalizedLeftThumb = 0f;
                CVRInputManager.Instance.fingerFullCurlNormalizedLeftIndex = 0f;
                CVRInputManager.Instance.fingerFullCurlNormalizedLeftMiddle = 0f;
                CVRInputManager.Instance.fingerFullCurlNormalizedLeftRing = 0f;
                CVRInputManager.Instance.fingerFullCurlNormalizedLeftPinky = 0f;

                // Right hand
                CVRInputManager.Instance.finger1StretchedRightThumb = 0f;
                CVRInputManager.Instance.finger2StretchedRightThumb = 0f;
                CVRInputManager.Instance.finger3StretchedRightThumb = 0f;
                CVRInputManager.Instance.fingerSpreadRightThumb = 0f;

                CVRInputManager.Instance.finger1StretchedRightIndex = 0f;
                CVRInputManager.Instance.finger2StretchedRightIndex = 0f;
                CVRInputManager.Instance.finger3StretchedRightIndex = 0f;
                CVRInputManager.Instance.fingerSpreadRightIndex = 0f;

                CVRInputManager.Instance.finger1StretchedRightMiddle = 0f;
                CVRInputManager.Instance.finger2StretchedRightMiddle = 0f;
                CVRInputManager.Instance.finger3StretchedRightMiddle = 0f;
                CVRInputManager.Instance.fingerSpreadRightMiddle = 0f;

                CVRInputManager.Instance.finger1StretchedRightRing = 0f;
                CVRInputManager.Instance.finger2StretchedRightRing = 0f;
                CVRInputManager.Instance.finger3StretchedRightRing = 0f;
                CVRInputManager.Instance.fingerSpreadRightRing = 0f;

                CVRInputManager.Instance.finger1StretchedRightPinky = 0f;
                CVRInputManager.Instance.finger2StretchedRightPinky = 0f;
                CVRInputManager.Instance.finger3StretchedRightPinky = 0f;
                CVRInputManager.Instance.fingerSpreadRightPinky = 0f;

                CVRInputManager.Instance.fingerFullCurlNormalizedRightThumb = 0f;
                CVRInputManager.Instance.fingerFullCurlNormalizedRightIndex = 0f;
                CVRInputManager.Instance.fingerFullCurlNormalizedRightMiddle = 0f;
                CVRInputManager.Instance.fingerFullCurlNormalizedRightRing = 0f;
                CVRInputManager.Instance.fingerFullCurlNormalizedRightPinky = 0f;
            }
        }
    }
}

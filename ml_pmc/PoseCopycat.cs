using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.IK.SubSystems;
using ABI_RC.Systems.MovementSystem;
using RootMotion.FinalIK;
using UnityEngine;

namespace ml_pmc
{
    [DisallowMultipleComponent]
    public class PoseCopycat : MonoBehaviour
    {
        static readonly Vector4 ms_pointVector = new Vector4(0f, 0f, 0f, 1f);

        static public PoseCopycat Instance { get; private set; } = null;
        static internal System.Action<bool> OnActivityChange;

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

        internal PoseCopycat()
        {
            if(Instance == null)
                Instance = this;
        }
        ~PoseCopycat()
        {
            if(Instance == this)
                Instance = null;
        }

        // Unity events
        void Update()
        {
            m_sitting = (MovementSystem.Instance.lastSeat != null);

            if(m_active && (m_puppetParser != null))
            {
                OverrideIK();

                if(m_puppetParser.HasAnimator())
                {
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

                        ref float[] l_curls = ref m_puppetParser.GetFingerCurls();

                        CVRInputManager.Instance.fingerCurlLeftThumb = l_curls[l_mirror ? 5 : 0];
                        CVRInputManager.Instance.fingerCurlLeftIndex = l_curls[l_mirror ? 6 : 1];
                        CVRInputManager.Instance.fingerCurlLeftMiddle = l_curls[l_mirror ? 7 : 2];
                        CVRInputManager.Instance.fingerCurlLeftRing = l_curls[l_mirror ? 8 : 3];
                        CVRInputManager.Instance.fingerCurlLeftPinky = l_curls[l_mirror ? 9 : 4];
                        CVRInputManager.Instance.fingerCurlRightThumb = l_curls[l_mirror ? 0 : 5];
                        CVRInputManager.Instance.fingerCurlRightIndex = l_curls[l_mirror ? 1 : 6];
                        CVRInputManager.Instance.fingerCurlRightMiddle = l_curls[l_mirror ? 2 : 7];
                        CVRInputManager.Instance.fingerCurlRightRing = l_curls[l_mirror ? 3 : 8];
                        CVRInputManager.Instance.fingerCurlRightPinky = l_curls[l_mirror ? 4 : 9];

                        IKSystem.Instance.FingerSystem.leftThumbCurl = l_curls[l_mirror ? 5 : 0];
                        IKSystem.Instance.FingerSystem.leftIndexCurl = l_curls[l_mirror ? 6 : 1];
                        IKSystem.Instance.FingerSystem.leftMiddleCurl = l_curls[l_mirror ? 7 : 2];
                        IKSystem.Instance.FingerSystem.leftRingCurl = l_curls[l_mirror ? 8 : 3];
                        IKSystem.Instance.FingerSystem.leftPinkyCurl = l_curls[l_mirror ? 9 : 4];
                        IKSystem.Instance.FingerSystem.rightThumbCurl = l_curls[l_mirror ? 0 : 5];
                        IKSystem.Instance.FingerSystem.rightIndexCurl = l_curls[l_mirror ? 1 : 6];
                        IKSystem.Instance.FingerSystem.rightMiddleCurl = l_curls[l_mirror ? 2 : 7];
                        IKSystem.Instance.FingerSystem.rightRingCurl = l_curls[l_mirror ? 3 : 8];
                        IKSystem.Instance.FingerSystem.rightPinkyCurl = l_curls[l_mirror ? 4 : 9];
                    }
                    else
                    {
                        if(m_fingerTracking)
                        {
                            RestoreFingerTracking();
                            m_fingerTracking = false;
                        }
                    }

                    Matrix4x4 l_offset = m_puppetParser.GetOffset();
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
                            Vector3 l_avatarPos = PlayerSetup.Instance._avatar.transform.position;
                            PlayerSetup.Instance.transform.rotation = l_result.rotation;
                            Vector3 l_dif = l_avatarPos - PlayerSetup.Instance._avatar.transform.position;
                            PlayerSetup.Instance.transform.position += l_dif;
                        }
                        else
                            PlayerSetup.Instance.transform.rotation = l_result.rotation;
                    }
                }
                else
                {
                    if(!m_puppetParser.IsWaitingAnimator())
                        SetTarget(null);
                }

                if(Vector3.Distance(this.transform.position, m_puppetParser.transform.position) > m_distanceLimit)
                    SetTarget(null);
            }
        }

        void LateUpdate()
        {
            if(m_active && (m_animator != null) && (m_puppetParser != null) && m_puppetParser.IsPoseParsed())
            {
                OverrideIK();

                m_puppetParser.GetPose().CopyTo(ref m_pose);
                if(Settings.MirrorPose)
                    Utils.MirrorPose(ref m_pose);
                m_poseHandler.SetHumanPose(ref m_pose);
            }
        }

        // Patches
        internal void OnAvatarClear()
        {
            m_inVr = Utils.IsInVR();

            if(m_puppetParser != null)
                Object.Destroy(m_puppetParser);
            m_puppetParser = null;

            m_animator = null;
            m_vrIk = null;
            m_lookAtIk = null;

            m_poseHandler?.Dispose();
            m_poseHandler = null;
            m_active = false;
            m_distanceLimit = float.MaxValue;
            m_fingerTracking = false;
            m_pose = new HumanPose();
        }
        internal void OnAvatarSetup()
        {
            m_animator = PlayerSetup.Instance._animator;
            m_vrIk = PlayerSetup.Instance._avatar.GetComponent<VRIK>();
            m_lookAtIk = PlayerSetup.Instance._avatar.GetComponent<LookAtIK>();

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
        public void SetTarget(GameObject p_target)
        {
            if(m_animator != null)
            {
                if(!m_active)
                {
                    if(p_target != null)
                    {
                        m_puppetParser = p_target.AddComponent<PuppetParser>();
                        m_distanceLimit = Utils.GetWorldMovementLimit();

                        m_active = true;
                        OnActivityChange?.Invoke(m_active);
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
                        OnActivityChange?.Invoke(m_active);
                    }
                }
            }
        }

        public bool IsActive() => m_active;
        public bool IsFingerTrackingActive() => m_fingerTracking;

        void OverrideIK()
        {
            if((m_vrIk != null) && !BodySystem.isCalibrating)
                BodySystem.TrackingPositionWeight = 0f;
        }
        void RestoreIK()
        {
            if((m_vrIk != null) && !BodySystem.isCalibrating)
            {
                BodySystem.TrackingPositionWeight = 1f;
                m_vrIk.solver.Reset();
            }
        }
        void RestoreFingerTracking()
        {
            CVRInputManager.Instance.individualFingerTracking = (m_inVr && Utils.AreKnucklesInUse() && !Utils.GetIndexGestureToggle());
            IKSystem.Instance.FingerSystem.controlActive = CVRInputManager.Instance.individualFingerTracking;

            if(!CVRInputManager.Instance.individualFingerTracking)
            {
                CVRInputManager.Instance.fingerCurlLeftThumb = 0f;
                CVRInputManager.Instance.fingerCurlLeftIndex = 0f;
                CVRInputManager.Instance.fingerCurlLeftMiddle = 0f;
                CVRInputManager.Instance.fingerCurlLeftRing = 0f;
                CVRInputManager.Instance.fingerCurlLeftPinky = 0f;
                CVRInputManager.Instance.fingerCurlRightThumb = 0f;
                CVRInputManager.Instance.fingerCurlRightIndex = 0f;
                CVRInputManager.Instance.fingerCurlRightMiddle = 0f;
                CVRInputManager.Instance.fingerCurlRightRing = 0f;
                CVRInputManager.Instance.fingerCurlRightPinky = 0f;

                IKSystem.Instance.FingerSystem.leftThumbCurl = 0f;
                IKSystem.Instance.FingerSystem.leftIndexCurl = 0f;
                IKSystem.Instance.FingerSystem.leftMiddleCurl = 0f;
                IKSystem.Instance.FingerSystem.leftRingCurl = 0f;
                IKSystem.Instance.FingerSystem.leftPinkyCurl = 0f;
                IKSystem.Instance.FingerSystem.rightThumbCurl = 0f;
                IKSystem.Instance.FingerSystem.rightIndexCurl = 0f;
                IKSystem.Instance.FingerSystem.rightMiddleCurl = 0f;
                IKSystem.Instance.FingerSystem.rightRingCurl = 0f;
                IKSystem.Instance.FingerSystem.rightPinkyCurl = 0f;
            }
        }
    }
}

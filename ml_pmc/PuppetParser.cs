using ABI_RC.Core.Player;
using UnityEngine;

namespace ml_pmc
{
    [DisallowMultipleComponent]
    class PuppetParser : MonoBehaviour
    {
        static readonly Vector4 ms_pointVector = new Vector4(0f, 0f, 0f, 1f);

        PuppetMaster m_puppetMaster = null;
        Animator m_animator = null;
        AnimatorCullingMode m_cullMode;
        float m_armatureScale = 1f;
        float m_armatureHeight = 0f;

        bool m_waitAnimator = true;
        HumanPoseHandler m_poseHandler = null;
        HumanPose m_pose;
        bool m_poseParsed = false;

        Matrix4x4 m_matrix = Matrix4x4.identity;
        Matrix4x4 m_offset = Matrix4x4.identity;

        bool m_sitting = false;
        float m_leftGesture = 0f;
        float m_rightGesture = 0f;
        bool m_fingerTracking = false;
        float[] m_fingerCurls = null;

        internal PuppetParser()
        {
            m_fingerCurls = new float[10];
        }

        // Unity events
        void Start()
        {
            m_puppetMaster = this.GetComponent<PuppetMaster>();
            m_matrix = this.transform.GetMatrix();
            StartCoroutine(WaitForAnimator());
        }

        void OnDestroy()
        {
            if(m_animator != null)
                m_animator.cullingMode = m_cullMode;

            m_poseHandler?.Dispose();
        }

        void Update()
        {
            if(m_puppetMaster != null)
            {
                m_sitting = m_puppetMaster.PlayerAvatarMovementDataInput.AnimatorSitting;
                m_leftGesture = m_puppetMaster.PlayerAvatarMovementDataInput.AnimatorGestureLeft;
                m_rightGesture = m_puppetMaster.PlayerAvatarMovementDataInput.AnimatorGestureRight;
                m_fingerTracking = m_puppetMaster.PlayerAvatarMovementDataInput.IndexUseIndividualFingers;
                if(m_fingerTracking)
                {
                    m_fingerCurls[0] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftThumbCurl;
                    m_fingerCurls[1] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftIndexCurl;
                    m_fingerCurls[2] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftMiddleCurl;
                    m_fingerCurls[3] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftRingCurl;
                    m_fingerCurls[4] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftPinkyCurl;
                    m_fingerCurls[5] = m_puppetMaster.PlayerAvatarMovementDataInput.RightThumbCurl;
                    m_fingerCurls[6] = m_puppetMaster.PlayerAvatarMovementDataInput.RightIndexCurl;
                    m_fingerCurls[7] = m_puppetMaster.PlayerAvatarMovementDataInput.RightMiddleCurl;
                    m_fingerCurls[8] = m_puppetMaster.PlayerAvatarMovementDataInput.RightRingCurl;
                    m_fingerCurls[9] = m_puppetMaster.PlayerAvatarMovementDataInput.RightPinkyCurl;
                }
            }

            if(!ReferenceEquals(m_animator, null))
            {
                if(m_animator != null)
                {
                    Matrix4x4 l_current = this.transform.GetMatrix();
                    m_offset = m_matrix.inverse * l_current;
                    m_matrix = l_current;
                }
                else
                    Reset();
            }
        }

        void LateUpdate()
        {
            if(m_animator != null)
            {
                m_poseHandler.GetHumanPose(ref m_pose);
                m_pose.bodyPosition *= m_armatureScale;
                m_pose.bodyPosition.y += m_armatureHeight;
                m_poseParsed = true;
            }
        }

        // Arbitrary
        System.Collections.IEnumerator WaitForAnimator()
        {
            while(m_puppetMaster.avatarObject == null)
                yield return null;

            while(m_animator == null)
            {
                m_animator = m_puppetMaster.avatarObject.GetComponent<Animator>();
                yield return null;
            }

            if(m_animator.isHuman)
            {
                m_cullMode = m_animator.cullingMode;
                m_animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                Transform l_hips = m_animator.GetBoneTransform(HumanBodyBones.Hips);
                if((l_hips != null) && (l_hips.parent != null))
                {
                    m_armatureScale = l_hips.parent.localScale.y;
                    m_armatureHeight = ((m_puppetMaster.transform.GetMatrix().inverse * l_hips.parent.GetMatrix()) * ms_pointVector).y;
                }

                m_poseHandler = new HumanPoseHandler(m_animator.avatar, m_animator.transform);
                m_matrix = this.transform.GetMatrix();
            }
            else
                Reset();

            m_waitAnimator = false;
        }

        void Reset()
        {
            m_animator = null;
            m_poseHandler?.Dispose();
            m_poseHandler = null;
            m_pose = new HumanPose();
            m_poseParsed = false;
            m_offset = Matrix4x4.identity;
            m_sitting = false;
            m_leftGesture = 0f;
            m_rightGesture = 0f;
        }

        public bool IsWaitingAnimator() => m_waitAnimator;
        public bool HasAnimator() => !ReferenceEquals(m_animator, null);
        public ref HumanPose GetPose() => ref m_pose;
        public bool IsPoseParsed() => m_poseParsed;
        public ref Matrix4x4 GetOffset() => ref m_offset;
        public bool IsSitting() => m_sitting;
        public float GetLeftGesture() => m_leftGesture;
        public float GetRightGesture() => m_rightGesture;
        public bool HasFingerTracking() => m_fingerTracking;
        public ref float[] GetFingerCurls() => ref m_fingerCurls;
    }
}

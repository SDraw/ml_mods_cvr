using ABI_RC.Core.Player;
using UnityEngine;

namespace ml_pmc
{
    [DisallowMultipleComponent]
    class PuppetParser : MonoBehaviour
    {
        internal PuppetMaster m_puppetMaster = null;
        Animator m_animator = null;
        AnimatorCullingMode m_cullMode;

        HumanPoseHandler m_poseHandler = null;
        HumanPose m_pose;

        Matrix4x4 m_matrix = Matrix4x4.identity;
        Matrix4x4 m_offset = Matrix4x4.identity;

        bool m_sitting = false;
        float m_leftGesture = 0f;
        float m_rightGesture = 0f;
        bool m_fingerTracking = false;
        readonly float[] m_fingerCurls = null;
        readonly float[] m_fingerSpreads = null;

        internal PuppetParser()
        {
            m_fingerCurls = new float[30];
            m_fingerSpreads = new float[10];
        }

        // Unity events
        void Start()
        {
            m_animator = this.GetComponent<Animator>();
            m_cullMode = m_animator.cullingMode;
            m_animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            m_poseHandler = new HumanPoseHandler(m_animator.avatar, m_animator.transform);
            m_poseHandler.GetHumanPose(ref m_pose);
            m_matrix = this.transform.GetMatrix();
        }

        void OnDestroy()
        {
            m_puppetMaster = null;
            if(m_animator != null)
                m_animator.cullingMode = m_cullMode;
            m_animator = null;

            m_poseHandler?.Dispose();
            m_poseHandler = null;
        }

        void Update()
        {
            if(m_puppetMaster != null)
            {
                m_sitting = m_puppetMaster.PlayerAvatarMovementDataInput.AnimatorSitting;
                m_leftGesture = m_puppetMaster.PlayerAvatarMovementDataInput.AnimatorGestureLeft;
                m_rightGesture = m_puppetMaster.PlayerAvatarMovementDataInput.AnimatorGestureRight;
                m_fingerTracking = m_puppetMaster.PlayerAvatarMovementDataInput.UseIndividualFingers;
                if(m_fingerTracking)
                {
                    m_fingerCurls[0] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftThumb1Stretched;
                    m_fingerCurls[1] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftThumb2Stretched;
                    m_fingerCurls[2] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftThumb3Stretched;
                    m_fingerSpreads[0] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftThumbSpread;

                    m_fingerCurls[3] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftIndex1Stretched;
                    m_fingerCurls[4] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftIndex2Stretched;
                    m_fingerCurls[5] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftIndex3Stretched;
                    m_fingerSpreads[1] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftIndexSpread;

                    m_fingerCurls[6] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftMiddle1Stretched;
                    m_fingerCurls[7] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftMiddle2Stretched;
                    m_fingerCurls[8] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftMiddle3Stretched;
                    m_fingerSpreads[2] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftMiddleSpread;

                    m_fingerCurls[9] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftRing1Stretched;
                    m_fingerCurls[10] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftRing2Stretched;
                    m_fingerCurls[11] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftRing3Stretched;
                    m_fingerSpreads[3] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftRingSpread;

                    m_fingerCurls[12] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftPinky1Stretched;
                    m_fingerCurls[13] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftPinky2Stretched;
                    m_fingerCurls[14] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftPinky3Stretched;
                    m_fingerSpreads[4] = m_puppetMaster.PlayerAvatarMovementDataInput.LeftPinkySpread;

                    m_fingerCurls[15] = m_puppetMaster.PlayerAvatarMovementDataInput.RightThumb1Stretched;
                    m_fingerCurls[16] = m_puppetMaster.PlayerAvatarMovementDataInput.RightThumb2Stretched;
                    m_fingerCurls[17] = m_puppetMaster.PlayerAvatarMovementDataInput.RightThumb3Stretched;
                    m_fingerSpreads[5] = m_puppetMaster.PlayerAvatarMovementDataInput.RightThumbSpread;

                    m_fingerCurls[18] = m_puppetMaster.PlayerAvatarMovementDataInput.RightIndex1Stretched;
                    m_fingerCurls[19] = m_puppetMaster.PlayerAvatarMovementDataInput.RightIndex2Stretched;
                    m_fingerCurls[20] = m_puppetMaster.PlayerAvatarMovementDataInput.RightIndex3Stretched;
                    m_fingerSpreads[6] = m_puppetMaster.PlayerAvatarMovementDataInput.RightIndexSpread;

                    m_fingerCurls[21] = m_puppetMaster.PlayerAvatarMovementDataInput.RightMiddle1Stretched;
                    m_fingerCurls[22] = m_puppetMaster.PlayerAvatarMovementDataInput.RightMiddle2Stretched;
                    m_fingerCurls[23] = m_puppetMaster.PlayerAvatarMovementDataInput.RightMiddle3Stretched;
                    m_fingerSpreads[7] = m_puppetMaster.PlayerAvatarMovementDataInput.RightMiddleSpread;

                    m_fingerCurls[24] = m_puppetMaster.PlayerAvatarMovementDataInput.RightRing1Stretched;
                    m_fingerCurls[25] = m_puppetMaster.PlayerAvatarMovementDataInput.RightRing2Stretched;
                    m_fingerCurls[26] = m_puppetMaster.PlayerAvatarMovementDataInput.RightRing3Stretched;
                    m_fingerSpreads[8] = m_puppetMaster.PlayerAvatarMovementDataInput.RightRingSpread;

                    m_fingerCurls[27] = m_puppetMaster.PlayerAvatarMovementDataInput.RightPinky1Stretched;
                    m_fingerCurls[28] = m_puppetMaster.PlayerAvatarMovementDataInput.RightPinky2Stretched;
                    m_fingerCurls[29] = m_puppetMaster.PlayerAvatarMovementDataInput.RightPinky3Stretched;
                    m_fingerSpreads[9] = m_puppetMaster.PlayerAvatarMovementDataInput.RightPinkySpread;
                }

                Matrix4x4 l_current = this.transform.GetMatrix();
                m_offset = m_matrix.inverse * l_current;
                m_matrix = l_current;
            }
        }

        void LateUpdate()
        {
            if((m_animator != null) && (m_poseHandler != null))
                m_poseHandler.GetHumanPose(ref m_pose);
        }

        public ref HumanPose GetPose() => ref m_pose;
        public ref Matrix4x4 GetLastOffset() => ref m_offset;
        public bool IsSitting() => m_sitting;
        public float GetLeftGesture() => m_leftGesture;
        public float GetRightGesture() => m_rightGesture;
        public bool HasFingerTracking() => m_fingerTracking;
        public ref readonly float[] GetFingerCurls() => ref m_fingerCurls;
        public ref readonly float[] GetFingerSpreads() => ref m_fingerSpreads;
    }
}

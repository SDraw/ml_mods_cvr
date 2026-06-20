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

        // Class methods
        public ref HumanPose GetPose() => ref m_pose;
        public ref Matrix4x4 GetLastOffset() => ref m_offset;
        public bool IsSitting() => m_sitting;
        public float GetLeftGesture() => m_leftGesture;
        public float GetRightGesture() => m_rightGesture;
        public bool HasFingerTracking() => m_fingerTracking;

        public void GetFingerMuscles(ref float[] target)
        {
            System.Array.Copy(m_pose.muscles, PlayerAvatarMovementData.MuscleGroups.LeftFingersStart, target, PlayerAvatarMovementData.MuscleGroups.LeftFingersStart, PlayerAvatarMovementData.MuscleGroups.LeftFingersCount);
            System.Array.Copy(m_pose.muscles, PlayerAvatarMovementData.MuscleGroups.RightFingersStart, target, PlayerAvatarMovementData.MuscleGroups.RightFingersStart, PlayerAvatarMovementData.MuscleGroups.RightFingersCount);
        }
    }
}

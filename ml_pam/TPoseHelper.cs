using UnityEngine;

namespace ml_pam
{
    class TPoseHelper
    {
        HumanPoseHandler m_poseHandler = null;
        HumanPose m_oldPose;
        HumanPose m_newPose;
        Vector3 m_hipsLocalPos = Vector3.zero;
        Transform m_hips = null;

        public void Assign(Animator p_animator)
        {
            if(m_poseHandler != null)
            {
                m_poseHandler = new HumanPoseHandler(p_animator.avatar, p_animator.transform);
                m_hips = p_animator.GetBoneTransform(HumanBodyBones.Hips);
            }
        }

        public void Unassign()
        {
            m_poseHandler?.Dispose();
            m_poseHandler = null;
            m_oldPose = new HumanPose();
            m_newPose = new HumanPose();
            m_hips = null;
            m_hipsLocalPos = Vector3.zero;
        }

        public void Apply()
        {
            if(m_hips != null)
                m_hipsLocalPos = m_hips.localPosition;

            if(m_poseHandler != null)
            {
                m_poseHandler.GetHumanPose(ref m_oldPose);
                m_newPose.bodyPosition = m_oldPose.bodyPosition;
                m_newPose.bodyRotation = m_oldPose.bodyRotation;
                m_newPose.muscles = new float[m_oldPose.muscles.Length];
                for(int i = 0, j = m_newPose.muscles.Length; i < j; i++)
                    m_newPose.muscles[i] = ABI_RC.Systems.IK.MusclePoses.TPoseMuscles[i];

                m_poseHandler.SetHumanPose(ref m_newPose);
            }
        }

        public void Restore()
        {
            if(m_poseHandler != null)
                m_poseHandler.SetHumanPose(ref m_oldPose);

            if(m_hips != null)
                m_hips.localPosition = m_hipsLocalPos;
        }
    }
}

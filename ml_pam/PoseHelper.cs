using UnityEngine;
using ABI_RC.Systems.IK;

namespace ml_pam
{
    static class PoseHelper
    {
        public static void ForceTPose(Animator p_animator)
        {
            if(p_animator.isHuman)
            {
                HumanPoseHandler l_handler = new HumanPoseHandler(p_animator.avatar, p_animator.transform);
                HumanPose l_pose = new HumanPose();
                l_handler.GetHumanPose(ref l_pose);

                for(int i = 0, j = Mathf.Min(l_pose.muscles.Length, MusclePoses.TPoseMuscles.Length); i < j; i++)
                    l_pose.muscles[i] = MusclePoses.TPoseMuscles[i];

                l_pose.bodyPosition = Vector3.up;
                l_pose.bodyRotation = Quaternion.identity;
                l_handler.SetHumanPose(ref l_pose);
                l_handler.Dispose();
            }
        }
    }
}

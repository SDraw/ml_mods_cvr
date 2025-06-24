using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Core.UI;
using ABI_RC.Systems.IK;
using RootMotion.FinalIK;
using System.Reflection;
using UnityEngine;

namespace ml_amt
{
    static class Utils
    {
        static readonly FieldInfo ms_hasToes = typeof(IKSolverVR).GetField("hasToes", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly FieldInfo ms_view = typeof(CohtmlControlledViewWrapper).GetField("_view", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool IsInVR() => ((MetaPort.Instance != null) && MetaPort.Instance.isUsingVr);

        public static bool HasToes(this IKSolverVR p_instance) => (bool)ms_hasToes?.GetValue(p_instance);

        public static void ExecuteScript(this CohtmlControlledViewWrapper p_instance, string p_script) => (ms_view?.GetValue(p_instance) as cohtml.Net.View)?.ExecuteScript(p_script);

        public static void SetAvatarTPose()
        {
            if(PlayerSetup.Instance.Animator.isHuman)
            {
                IKSystem.Instance.SetAvatarPose(IKSystem.AvatarPose.TPose);
                PlayerSetup.Instance.AvatarTransform.localPosition = Vector3.zero;
                PlayerSetup.Instance.AvatarTransform.localRotation = Quaternion.identity;
            }
        }

        // Engine extensions
        public static Matrix4x4 GetMatrix(this Transform p_transform, bool p_pos = true, bool p_rot = true, bool p_scl = false)
        {
            return Matrix4x4.TRS(p_pos ? p_transform.position : Vector3.zero, p_rot ? p_transform.rotation : Quaternion.identity, p_scl ? p_transform.localScale : Vector3.one);
        }
    }
}

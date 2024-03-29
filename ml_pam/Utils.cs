using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Core.UI;
using ABI_RC.Systems.IK;
using System.Reflection;
using UnityEngine;

namespace ml_pam
{
    static class Utils
    {
        static readonly FieldInfo ms_view = typeof(CohtmlControlledViewWrapper).GetField("_view", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsInVR() => ((MetaPort.Instance != null) && MetaPort.Instance.isUsingVr);

        public static void ExecuteScript(this CohtmlControlledViewWrapper p_instance, string p_script) => ((cohtml.Net.View)ms_view.GetValue(p_instance)).ExecuteScript(p_script);

        public static void SetAvatarTPose()
        {
            IKSystem.Instance.SetAvatarPose(IKSystem.AvatarPose.TPose);
            PlayerSetup.Instance._avatar.transform.localPosition = Vector3.zero;
            PlayerSetup.Instance._avatar.transform.localRotation = Quaternion.identity;
        }

        // Extensions
        public static Matrix4x4 GetMatrix(this Transform p_transform, bool p_pos = true, bool p_rot = true, bool p_scl = false)
        {
            return Matrix4x4.TRS(p_pos ? p_transform.position : Vector3.zero, p_rot ? p_transform.rotation : Quaternion.identity, p_scl ? p_transform.localScale : Vector3.one);
        }
    }
}

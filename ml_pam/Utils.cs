using ABI_RC.Core.UI;
using System.Reflection;
using UnityEngine;

namespace ml_pam
{
    static class Utils
    {
        static FieldInfo ms_cohtmlView = typeof(CohtmlControlledViewDisposable).GetField("_view", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsInVR() => ((ABI_RC.Core.Savior.CheckVR.Instance != null) && ABI_RC.Core.Savior.CheckVR.Instance.hasVrDeviceLoaded);

        public static void ExecuteScript(this CohtmlControlledViewDisposable p_instance, string p_script) => ((cohtml.Net.View)ms_cohtmlView.GetValue(p_instance))?.ExecuteScript(p_script);

        // Extensions
        public static Matrix4x4 GetMatrix(this Transform p_transform, bool p_pos = true, bool p_rot = true, bool p_scl = false)
        {
            return Matrix4x4.TRS(p_pos ? p_transform.position : Vector3.zero, p_rot ? p_transform.rotation : Quaternion.identity, p_scl ? p_transform.localScale : Vector3.one);
        }
    }
}

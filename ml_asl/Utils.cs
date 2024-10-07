using ABI_RC.Core.UI;
using System.Reflection;
using UnityEngine;

namespace ml_asl
{
    static class Utils
    {
        static readonly FieldInfo ms_view = typeof(CohtmlControlledViewWrapper).GetField("_view", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void ExecuteScript(this CohtmlControlledViewWrapper p_instance, string p_script) => (ms_view?.GetValue(p_instance) as cohtml.Net.View)?.ExecuteScript(p_script);

        // Extensions
        public static Matrix4x4 GetMatrix(this Transform p_transform, bool p_pos = true, bool p_rot = true, bool p_scl = false)
        {
            return Matrix4x4.TRS(p_pos ? p_transform.position : Vector3.zero, p_rot ? p_transform.rotation : Quaternion.identity, p_scl ? p_transform.localScale : Vector3.one);
        }
    }
}

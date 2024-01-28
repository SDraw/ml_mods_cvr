using ABI.CCK.Components;
using ABI_RC.Core.UI;
using System.Reflection;
using UnityEngine;

namespace ml_dht
{
    static class Utils
    {
        static readonly object[] ms_emptyArray = new object[0];
        static readonly FieldInfo ms_view = typeof(CohtmlControlledViewWrapper).GetField("_view", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo ms_updateShapesLocal = typeof(CVRFaceTracking).GetMethod("UpdateShapesLocal", BindingFlags.NonPublic | BindingFlags.Instance);

        static public void ExecuteScript(this CohtmlControlledViewWrapper p_instance, string p_script) => ((cohtml.Net.View)ms_view.GetValue(p_instance)).ExecuteScript(p_script);

        static public void UpdateShapesLocal_Private(this CVRFaceTracking p_instance) => ms_updateShapesLocal?.Invoke(p_instance, ms_emptyArray); 

        public static Matrix4x4 GetMatrix(this Transform p_transform, bool p_pos = true, bool p_rot = true, bool p_scl = false)
        {
            return Matrix4x4.TRS(p_pos ? p_transform.position : Vector3.zero, p_rot ? p_transform.rotation : Quaternion.identity, p_scl ? p_transform.localScale : Vector3.one);
        }
    }
}

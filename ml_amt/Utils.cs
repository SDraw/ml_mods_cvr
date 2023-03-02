using UnityEngine;
using System.Reflection;

namespace ml_amt
{
    static class Utils
    {
        static MethodInfo ms_getSineKeyframes = typeof(RootMotion.FinalIK.IKSolverVR).GetMethod("GetSineKeyframes", BindingFlags.NonPublic | BindingFlags.Static);

        public static bool IsInVR() => ((ABI_RC.Core.Savior.CheckVR.Instance != null) && ABI_RC.Core.Savior.CheckVR.Instance.hasVrDeviceLoaded);

        // Extensions
        public static Matrix4x4 GetMatrix(this Transform p_transform, bool p_pos = true, bool p_rot = true, bool p_scl = false)
        {
            return Matrix4x4.TRS(p_pos ? p_transform.position : Vector3.zero, p_rot ? p_transform.rotation : Quaternion.identity, p_scl ? p_transform.localScale : Vector3.one);
        }

        public static Keyframe[] GetSineKeyframes(float p_mag)
        {
            return (Keyframe[])ms_getSineKeyframes.Invoke(null, new object[] { p_mag });
        }
    }
}

using ABI_RC.Core.Player;
using System.Linq;
using UnityEngine;

namespace ml_lme
{
    static class Utils
    {
        public static bool AreKnucklesInUse() => PlayerSetup.Instance._trackerManager.trackerNames.Contains("knuckles");
        
        public static Matrix4x4 GetMatrix(this Transform p_transform, bool p_pos = true, bool p_rot = true, bool p_scl = false)
        {
            return Matrix4x4.TRS(p_pos ? p_transform.position : Vector3.zero, p_rot ? p_transform.rotation : Quaternion.identity, p_scl ? p_transform.localScale : Vector3.one);
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }
}

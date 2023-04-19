using ABI_RC.Core.Player;
using System.Reflection;
using UnityEngine;

namespace ml_dht
{
    static class Utils
    {
        static FieldInfo ms_emotePlaying = typeof(PlayerSetup).GetField("_emotePlaying", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsEmotePlaying(this PlayerSetup p_instance) => (bool)ms_emotePlaying.GetValue(p_instance);

        public static Matrix4x4 GetMatrix(this Transform p_transform, bool p_pos = true, bool p_rot = true, bool p_scl = false)
        {
            return Matrix4x4.TRS(p_pos ? p_transform.position : Vector3.zero, p_rot ? p_transform.rotation : Quaternion.identity, p_scl ? p_transform.localScale : Vector3.one);
        }
    }
}

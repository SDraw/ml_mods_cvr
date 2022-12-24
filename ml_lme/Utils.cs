using ABI_RC.Core.Player;
using System.Linq;
using UnityEngine;

namespace ml_lme
{
    static class Utils
    {
        static readonly Quaternion ms_hmdRotationFix = new Quaternion(0f, 0.7071068f, 0.7071068f, 0f);
        static readonly Quaternion ms_screentopRotationFix = new Quaternion(0f, 0f, -1f, 0f);
        
        public static bool AreKnucklesInUse() => PlayerSetup.Instance._trackerManager.trackerNames.Contains("knuckles");

        public static bool IsInVR() => ((ABI_RC.Core.Savior.CheckVR.Instance != null) && ABI_RC.Core.Savior.CheckVR.Instance.hasVrDeviceLoaded);

        public static Matrix4x4 GetMatrix(this Transform p_transform, bool p_pos = true, bool p_rot = true, bool p_scl = false)
        {
            return Matrix4x4.TRS(p_pos ? p_transform.position : Vector3.zero, p_rot ? p_transform.rotation : Quaternion.identity, p_scl ? p_transform.lossyScale : Vector3.one);
        }
        
        public static void LeapToUnity(ref Vector3 p_pos, ref Quaternion p_rot, Settings.LeapTrackingMode p_mode)
        {
            p_pos *= 0.001f;
            p_pos.z *= -1f;
            p_rot.x *= -1f;
            p_rot.y *= -1f;

            switch(p_mode)
            {
                case Settings.LeapTrackingMode.Screentop:
                {
                    p_pos.x *= -1f;
                    p_pos.y *= -1f;
                    p_rot = (ms_screentopRotationFix * p_rot);
                }
                break;

                case Settings.LeapTrackingMode.HMD:
                {
                    p_pos.x *= -1f;
                    Swap(ref p_pos.y, ref p_pos.z);
                    p_rot = (ms_hmdRotationFix * p_rot);
                }
                break;
            }
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }
}

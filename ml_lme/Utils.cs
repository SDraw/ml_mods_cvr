using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Core.UI;
using ABI_RC.Systems.InputManagement;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ml_lme
{
    static class Utils
    {
        static readonly Quaternion ms_hmdRotationFix = new Quaternion(0f, 0.7071068f, 0.7071068f, 0f);
        static readonly Quaternion ms_screentopRotationFix = new Quaternion(0f, 0f, -1f, 0f);

        static FieldInfo ms_cohtmlView = typeof(CohtmlControlledViewDisposable).GetField("_view", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsInVR() => ((CheckVR.Instance != null) && CheckVR.Instance.hasVrDeviceLoaded);
        public static bool AreKnucklesInUse() => ((CVRInputManager.Instance._leftController == ABI_RC.Systems.InputManagement.XR.EXRControllerType.Index) || (CVRInputManager.Instance._rightController == ABI_RC.Systems.InputManagement.XR.EXRControllerType.Index));
        public static bool IsLeftHandTracked() => (CVRInputManager.Instance._leftController != ABI_RC.Systems.InputManagement.XR.EXRControllerType.None);
        public static bool IsRightHandTracked() => (CVRInputManager.Instance._rightController != ABI_RC.Systems.InputManagement.XR.EXRControllerType.None);

        public static Matrix4x4 GetMatrix(this Transform p_transform, bool p_pos = true, bool p_rot = true, bool p_scl = false)
        {
            return Matrix4x4.TRS(p_pos ? p_transform.position : Vector3.zero, p_rot ? p_transform.rotation : Quaternion.identity, p_scl ? p_transform.lossyScale : Vector3.one);
        }

        public static void ShowHUDNotification(string p_title, string p_message, string p_small = "", bool p_immediate = false)
        {
            if(CohtmlHud.Instance != null)
            {
                if(p_immediate)
                    CohtmlHud.Instance.ViewDropTextImmediate(p_title, p_message, p_small);
                else
                    CohtmlHud.Instance.ViewDropText(p_title, p_message, p_small);
            }
        }

        public static void ExecuteScript(this CohtmlControlledViewDisposable p_instance, string p_script) => ((cohtml.Net.View)ms_cohtmlView.GetValue(p_instance))?.ExecuteScript(p_script);

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

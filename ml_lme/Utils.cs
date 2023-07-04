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

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        public static float InverseLerpUnclamped(float a, float b, float value)
        {
            if(a != b)
                return (value - a) / (b - a);
            return 0f;
        }
    }
}

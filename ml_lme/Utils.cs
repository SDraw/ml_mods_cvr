using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Core.UI;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ml_lme
{
    static class Utils
    {
        static readonly Quaternion ms_hmdRotationFix = new Quaternion(0f, 0.7071068f, 0.7071068f, 0f);
        static readonly Quaternion ms_screentopRotationFix = new Quaternion(0f, 0f, -1f, 0f);
        static readonly FieldInfo ms_indexGestureToggle = typeof(InputModuleSteamVR).GetField("_steamVrIndexGestureToggleValue", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool IsInVR() => ((CheckVR.Instance != null) && CheckVR.Instance.hasVrDeviceLoaded);
        public static bool AreKnucklesInUse() => PlayerSetup.Instance._trackerManager.trackerNames.Contains("knuckles");
        public static bool GetIndexGestureToggle(this InputModuleSteamVR p_module) => (bool)ms_indexGestureToggle.GetValue(p_module);
        public static bool IsLeftHandTracked() => ((VRTrackerManager.Instance.leftHand != null) && VRTrackerManager.Instance.leftHand.active);
        public static bool IsRightHandTracked() => ((VRTrackerManager.Instance.rightHand != null) && VRTrackerManager.Instance.rightHand.active);

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

using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Core.UI;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.InputManagement;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ml_lme
{
    static class Utils
    {
        static readonly FieldInfo ms_view = typeof(CohtmlControlledViewWrapper).GetField("_view", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly FieldInfo ms_vrActive = typeof(ControllerRay).GetField("vrActive", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly FieldInfo ms_inputModules = typeof(CVRInputManager).GetField("_inputModules", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool IsInVR() => ((MetaPort.Instance != null) && MetaPort.Instance.isUsingVr);
        public static bool AreKnucklesInUse() => ((CVRInputManager.Instance._leftController == ABI_RC.Systems.InputManagement.XR.eXRControllerType.Index) || (CVRInputManager.Instance._rightController == ABI_RC.Systems.InputManagement.XR.eXRControllerType.Index));
        public static bool IsLeftHandTracked() => (CVRInputManager.Instance._leftController != ABI_RC.Systems.InputManagement.XR.eXRControllerType.None);
        public static bool IsRightHandTracked() => (CVRInputManager.Instance._rightController != ABI_RC.Systems.InputManagement.XR.eXRControllerType.None);

        public static Matrix4x4 GetMatrix(this Transform p_transform, bool p_pos = true, bool p_rot = true, bool p_scl = false)
        {
            return Matrix4x4.TRS(p_pos ? p_transform.position : Vector3.zero, p_rot ? p_transform.rotation : Quaternion.identity, p_scl ? p_transform.lossyScale : Vector3.one);
        }

        public static void ShowHUDNotification(string p_title, string p_message, string p_small = "", bool p_immediate = false)
        {
            if(CohtmlHud.Instance != null)
            {
                if(p_immediate)
                    CohtmlHud.Instance.ViewDropTextImmediate(p_title, p_message, p_small, "", false);
                else
                    CohtmlHud.Instance.ViewDropText(p_title, p_message, p_small, "", false);
            }
        }

        public static void SetVRActive(this ControllerRay p_instance, bool p_state) => ms_vrActive?.SetValue(p_instance, p_state);

        public static void SetModuleAsLast(this CVRInputManager p_instance, CVRInputModule p_module)
        {
            List<CVRInputModule> l_modules = ms_inputModules?.GetValue(p_instance) as List<CVRInputModule>;
            if(l_modules != null)
            {
                int l_lastIndex = l_modules.Count - 1;
                int l_index = l_modules.FindIndex(p => p == p_module);
                if((l_index != -1) && (l_index != l_lastIndex))
                {
                    l_modules[l_index] = l_modules[l_lastIndex];
                    l_modules[l_lastIndex] = p_module;
                }
            }
        }

        public static void ExecuteScript(this CohtmlControlledViewWrapper p_instance, string p_script) => (ms_view?.GetValue(p_instance) as cohtml.Net.View)?.ExecuteScript(p_script);

        public static void SetAvatarTPose()
        {
            IKSystem.Instance.SetAvatarPose(IKSystem.AvatarPose.TPose);
            PlayerSetup.Instance.AvatarTransform.localPosition = Vector3.zero;
            PlayerSetup.Instance.AvatarTransform.localRotation = Quaternion.identity;
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        public static float InverseLerpUnclamped(float a, float b, float value)
        {
            if(!Mathf.Approximately(a, b))
                return (value - a) / (b - a);
            return 0f;
        }
    }
}

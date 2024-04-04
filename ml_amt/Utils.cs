﻿using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Core.UI;
using ABI_RC.Systems.IK;
using RootMotion.FinalIK;
using System.Reflection;
using UnityEngine;

namespace ml_amt
{
    static class Utils
    {
        static readonly FieldInfo ms_hasToes = typeof(IKSolverVR).GetField("hasToes", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo ms_view = typeof(CohtmlControlledViewWrapper).GetField("_view", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsInVR() => ((MetaPort.Instance != null) && MetaPort.Instance.isUsingVr);

        public static bool HasToes(this IKSolverVR p_instance) => (bool)ms_hasToes.GetValue(p_instance);

        public static bool IsWorldSafe() => ((CVRWorld.Instance != null) && CVRWorld.Instance.allowFlying);
        public static float GetWorldMovementLimit()
        {
            float l_result = 1f;
            if(CVRWorld.Instance != null)
            {
                l_result = CVRWorld.Instance.baseMovementSpeed;
                l_result *= CVRWorld.Instance.sprintMultiplier;
                l_result *= CVRWorld.Instance.inAirMovementMultiplier;
                l_result *= CVRWorld.Instance.flyMultiplier;
            }
            return l_result;
        }

        public static void ExecuteScript(this CohtmlControlledViewWrapper p_instance, string p_script) => ((cohtml.Net.View)ms_view.GetValue(p_instance)).ExecuteScript(p_script);

        public static void SetAvatarTPose()
        {
            IKSystem.Instance.SetAvatarPose(IKSystem.AvatarPose.TPose);
            PlayerSetup.Instance._avatar.transform.localPosition = Vector3.zero;
            PlayerSetup.Instance._avatar.transform.localRotation = Quaternion.identity;
        }

        // Engine extensions
        public static Matrix4x4 GetMatrix(this Transform p_transform, bool p_pos = true, bool p_rot = true, bool p_scl = false)
        {
            return Matrix4x4.TRS(p_pos ? p_transform.position : Vector3.zero, p_rot ? p_transform.rotation : Quaternion.identity, p_scl ? p_transform.localScale : Vector3.one);
        }
    }
}

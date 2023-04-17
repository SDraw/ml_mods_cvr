using ABI.CCK.Components;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.MovementSystem;
using System.Reflection;
using UnityEngine;

namespace ml_prm
{
    static class Utils
    {
        static readonly FieldInfo ms_grounded = typeof(MovementSystem).GetField("_isGrounded", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsInVR() => ((CheckVR.Instance != null) && CheckVR.Instance.hasVrDeviceLoaded);
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

        public static bool IsGrounded(this MovementSystem p_instance) => (bool)ms_grounded.GetValue(p_instance);

        public static void CopyGlobal(this Transform p_source, Transform p_target)
        {
            p_target.position = p_source.position;
            p_target.rotation = p_source.rotation;
        }
    }
}

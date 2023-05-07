using ABI.CCK.Components;
using ABI_RC.Systems.MovementSystem;
using RootMotion.FinalIK;
using System.Reflection;
using UnityEngine;

namespace ml_amt
{
    static class Utils
    {
        static readonly FieldInfo ms_grounded = typeof(MovementSystem).GetField("_isGrounded", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo ms_groundedRaw = typeof(MovementSystem).GetField("_isGroundedRaw", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo ms_hasToes = typeof(IKSolverVR).GetField("hasToes", BindingFlags.NonPublic | BindingFlags.Instance);
        static MethodInfo ms_getSineKeyframes = typeof(IKSolverVR).GetMethod("GetSineKeyframes", BindingFlags.NonPublic | BindingFlags.Static);

        public static bool IsInVR() => ((ABI_RC.Core.Savior.CheckVR.Instance != null) && ABI_RC.Core.Savior.CheckVR.Instance.hasVrDeviceLoaded);

        public static bool IsGrounded(this MovementSystem p_instance) => (bool)ms_grounded.GetValue(MovementSystem.Instance);
        public static bool IsGroundedRaw(this MovementSystem p_instance) => (bool)ms_groundedRaw.GetValue(MovementSystem.Instance);

        public static bool HasToes(this IKSolverVR p_instance) => (bool)ms_hasToes.GetValue(p_instance);
        public static Keyframe[] GetSineKeyframes(float p_mag)
        {
            return (Keyframe[])ms_getSineKeyframes.Invoke(null, new object[] { p_mag });
        }

        public static bool IsWorldSafe() => ((CVRWorld.Instance != null) && CVRWorld.Instance.allowFlying);
        public static float GetWorldJumpHeight()
        {
            float l_result = 1f;
            if(CVRWorld.Instance != null)
                l_result = CVRWorld.Instance.jumpHeight;
            return l_result;
        }
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

        // Engine extensions
        public static Matrix4x4 GetMatrix(this Transform p_transform, bool p_pos = true, bool p_rot = true, bool p_scl = false)
        {
            return Matrix4x4.TRS(p_pos ? p_transform.position : Vector3.zero, p_rot ? p_transform.rotation : Quaternion.identity, p_scl ? p_transform.localScale : Vector3.one);
        }
    }
}

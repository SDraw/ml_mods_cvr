using ABI_RC.Core;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.Movement;
using ABI_RC.Systems.Safety.AdvancedSafety;
using UnityEngine;

namespace ml_ppu
{
    static class Utils
    {
        public static bool IsInVR() => ((MetaPort.Instance != null) && MetaPort.Instance.isUsingVr);

        public static Matrix4x4 GetMatrix(this Transform p_transform, bool p_pos = true, bool p_rot = true, bool p_scl = false)
        {
            return Matrix4x4.TRS(p_pos ? p_transform.position : Vector3.zero, p_rot ? p_transform.rotation : Quaternion.identity, p_scl ? p_transform.lossyScale : Vector3.one);
        }

        // Remade method to remove Q-E-Q conversions to prevent bad angles
        public static void TeleportPlayerTo(this BetterBetterCharacterController p_instance, Vector3 p_targetPos, Quaternion p_targetRot, bool p_interpolate, bool p_updateGround, bool p_preserveVelocity = false)
        {
            Quaternion l_quaternion = p_targetRot * Quaternion.Inverse(PlayerSetup.Instance.GetPlayerRotation());
            Quaternion l_newRotation = l_quaternion * p_instance.characterMovement.rotation;
            if(l_newRotation.eulerAngles.IsAbsurd())
            {
                CommonTools.LogAuto(CommonTools.LogLevelType_t.Warning, "Attempted to teleport using an absurd rotation. Ignoring it...", "", "Assets\\ABI RC\\Systems\\Movement\\BetterBetterCharacterController.cs", "TeleportPlayerTo", 1845);
                return;
            }
            p_interpolate = false;
            p_instance.TeleportRotation(l_newRotation, p_interpolate);
            p_instance.TeleportPlayerTo(p_targetPos, p_interpolate, p_updateGround, p_preserveVelocity, l_quaternion);
        }
    }
}

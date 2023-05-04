using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ml_pmc
{
    static class Utils
    {
        static readonly FieldInfo ms_indexGestureToggle = typeof(InputModuleSteamVR).GetField("_steamVrIndexGestureToggleValue", BindingFlags.Instance | BindingFlags.NonPublic);

        static readonly (int, int)[] ms_sideMuscles = new (int, int)[]
        {
            (29,21), (30,22), (31,23), (32,24), (33,25), (34,26), (35,27), (36,28),
            (46,37), (47,38), (48,39), (49,40), (50,41), (51,42), (52,43), (53,44), (54,45),
            (75,55), (76,56), (77,57), (78,58), (79,59), (80,60), (81,61), (82,62), (83,63), (84,64),
            (85,65), (86,66), (87,67), (88,68), (89, 69), (90,70), (91,71), (92,72), (93,73), (94,74)
        };
        static readonly int[] ms_centralMuscles = new int[] { 1, 2, 4, 5, 7, 8, 10, 11, 13, 14, 16, 18, 20 };

        public static bool IsInVR() => ((CheckVR.Instance != null) && CheckVR.Instance.hasVrDeviceLoaded);
        public static bool AreKnucklesInUse() => PlayerSetup.Instance._trackerManager.trackerNames.Contains("knuckles");
        public static bool GetIndexGestureToggle() => (bool)ms_indexGestureToggle.GetValue(CVRInputManager.Instance.GetComponent<InputModuleSteamVR>());

        public static bool IsWorldSafe() => ((CVRWorld.Instance != null) && CVRWorld.Instance.allowFlying);
        public static bool IsCombatSafe() => ((CombatSystem.Instance == null) || !CombatSystem.Instance.isDown);

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

        public static Matrix4x4 GetMatrix(this Transform p_transform, bool p_pos = true, bool p_rot = true, bool p_scl = false)
        {
            return Matrix4x4.TRS(p_pos ? p_transform.position : Vector3.zero, p_rot ? p_transform.rotation : Quaternion.identity, p_scl ? p_transform.lossyScale : Vector3.one);
        }

        public static void CopyTo(this HumanPose p_source, ref HumanPose p_target)
        {
            p_target.bodyPosition = p_source.bodyPosition;
            p_target.bodyRotation = p_source.bodyRotation;

            int l_count = Mathf.Min(p_source.muscles.Length, p_target.muscles.Length);
            for(int i = 0; i < l_count; i++)
                p_target.muscles[i] = p_source.muscles[i];
        }

        public static void MirrorPose(ref HumanPose p_pose)
        {
            int l_count = p_pose.muscles.Length;
            foreach(var l_pair in ms_sideMuscles)
            {
                if((l_count > l_pair.Item1) && (l_count > l_pair.Item2))
                {
                    float l_temp = p_pose.muscles[l_pair.Item1];
                    p_pose.muscles[l_pair.Item1] = p_pose.muscles[l_pair.Item2];
                    p_pose.muscles[l_pair.Item2] = l_temp;
                }
            }
            foreach(int l_index in ms_centralMuscles)
            {
                if(l_count > l_index)
                    p_pose.muscles[l_index] *= -1f;
            }

            p_pose.bodyRotation.x *= -1f;
            p_pose.bodyRotation.w *= -1f;
            p_pose.bodyPosition.x *= -1f;
        }
    }
}

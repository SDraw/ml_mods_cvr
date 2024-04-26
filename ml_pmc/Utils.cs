using ABI.CCK.Components;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.InputManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ml_pmc
{
    static class Utils
    {
        static readonly (int, int)[] ms_sideMuscles = new (int, int)[]
        {
            (29,21), (30,22), (31,23), (32,24), (33,25), (34,26), (35,27), (36,28),
            (46,37), (47,38), (48,39), (49,40), (50,41), (51,42), (52,43), (53,44), (54,45),
            (75,55), (76,56), (77,57), (78,58), (79,59), (80,60), (81,61), (82,62), (83,63), (84,64),
            (85,65), (86,66), (87,67), (88,68), (89, 69), (90,70), (91,71), (92,72), (93,73), (94,74)
        };
        static readonly int[] ms_centralMuscles = new int[] { 1, 2, 4, 5, 7, 8, 10, 11, 13, 14, 16, 18, 20 };

        public static bool IsInVR() => ((MetaPort.Instance != null) && MetaPort.Instance.isUsingVr);
        public static bool AreKnucklesInUse() => ((CVRInputManager.Instance._leftController == ABI_RC.Systems.InputManagement.XR.eXRControllerType.Index) || (CVRInputManager.Instance._rightController == ABI_RC.Systems.InputManagement.XR.eXRControllerType.Index));

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

        public static bool IsInSight(CapsuleCollider p_source, CapsuleCollider p_target, float p_limit)
        {
            bool l_result = false;
            if((p_source != null) && (p_target != null))
            {
                Ray l_ray = new Ray();
                l_ray.origin = p_source.bounds.center;
                l_ray.direction = p_target.bounds.center - l_ray.origin;
                List<RaycastHit> l_hits = Physics.RaycastAll(l_ray, p_limit).ToList();
                if(l_hits.Count > 0)
                {
                    l_hits.RemoveAll(hit => hit.collider.gameObject.layer == LayerMask.NameToLayer("UI Internal")); // Somehow layer mask in RaycastAll just ignores players entirely
                    l_hits.RemoveAll(hit => hit.collider.gameObject.layer == LayerMask.NameToLayer("PlayerLocal"));
                    l_hits.RemoveAll(hit => hit.collider.gameObject.layer == LayerMask.NameToLayer("PlayerClone"));
                    l_hits.Sort((a, b) => ((a.distance < b.distance) ? -1 : 1));
                    l_result = (l_hits.First().collider.gameObject.transform.root == p_target.transform.root);
                }
            }
            return l_result;
        }
    }
}

using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.Movement;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using ABI_RC.Core.InteractionSystem;

namespace ml_prm
{
    static class Utils
    {
        static readonly FieldInfo ms_touchingVolumes = typeof(BetterBetterCharacterController).GetField("_currentlyTouchingFluidVolumes", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly FieldInfo ms_referencePoints = typeof(PhysicsInfluencer).GetField("_referencePoints", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly FieldInfo ms_influencerTouchingVolumes = typeof(PhysicsInfluencer).GetField("_touchingVolumes", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly FieldInfo ms_influencerSubmergedColliders = typeof(PhysicsInfluencer).GetField("_submergedColliders", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly FieldInfo ms_lastCVRSeat = typeof(BetterBetterCharacterController).GetField("_lastCvrSeat", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void ClearFluidVolumes(this BetterBetterCharacterController p_instance) => (ms_touchingVolumes?.GetValue(p_instance) as List<FluidVolume>)?.Clear();

        public static bool IsReady(this PhysicsInfluencer p_instance)
        {
            return ((ms_referencePoints.GetValue(p_instance) as List<Vector3>).Count > 0);
        }
        public static void ClearFluidVolumes(this PhysicsInfluencer p_instance)
        {
            (ms_influencerTouchingVolumes.GetValue(p_instance) as List<FluidVolume>)?.Clear();
            (ms_influencerSubmergedColliders.GetValue(p_instance) as Dictionary<FluidVolume, int>)?.Clear();
        }

        public static bool IsObjectInArray(object p_obj, object[] p_enumeration) => p_enumeration.Contains(p_obj);

        public static CVRSeat GetCurrentSeat(this BetterBetterCharacterController p_instance) => (ms_lastCVRSeat?.GetValue(p_instance) as CVRSeat);

        // Unity specific
        public static void CopyGlobal(this Transform p_source, Transform p_target)
        {
            p_target.position = p_source.position;
            p_target.rotation = p_source.rotation;
        }
    }
}

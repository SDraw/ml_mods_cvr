using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.Movement;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using ABI_RC.Systems.IK.SubSystems;
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

        public static void CopyGlobal(this Transform p_source, Transform p_target)
        {
            p_target.position = p_source.position;
            p_target.rotation = p_source.rotation;
        }

        public static bool IsReady(this PhysicsInfluencer p_instance)
        {
            return ((ms_referencePoints.GetValue(p_instance) as List<Vector3>).Count > 0);
        }
        public static void ClearFluidVolumes(this PhysicsInfluencer p_instance)
        {
            (ms_influencerTouchingVolumes.GetValue(p_instance) as List<FluidVolume>)?.Clear();
            (ms_influencerSubmergedColliders.GetValue(p_instance) as Dictionary<FluidVolume, int>)?.Clear();
        }

        public static void SetAvatarTPose()
        {
            IKSystem.Instance.SetAvatarPose(IKSystem.AvatarPose.TPose);
            PlayerSetup.Instance._avatar.transform.localPosition = Vector3.zero;
            PlayerSetup.Instance._avatar.transform.localRotation = Quaternion.identity;
        }

        public static bool IsInEnumeration(object p_obj, object[] p_enumeration) => p_enumeration.Contains(p_obj);

        public static bool IsLeftGrabPointerActive(this PuppetMaster p_source)
        {
            return p_source._playerAvatarMovementDataCurrent.IsLeftHandGrabbing();
        }

        public static bool IsRightGrabPointerActive(this PuppetMaster p_source)
        {
            return p_source._playerAvatarMovementDataCurrent.IsRightHandGrabbing();
        }

        public static CVRSeat GetCurrentSeat(this BetterBetterCharacterController p_instance) => (ms_lastCVRSeat?.GetValue(p_instance) as CVRSeat);

        public static bool IsInRange(float p_value, float p_min, float p_max) => ((p_min <= p_value) && (p_value <= p_max));
    }
}

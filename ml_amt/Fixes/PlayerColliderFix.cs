using ABI_RC.Core.Player;
using ABI_RC.Systems.MovementSystem;
using System;
using System.Reflection;
using UnityEngine;

namespace ml_amt.Fixes
{
    static class PlayerColliderFix
    {
        static FieldInfo ms_initialAvatarHeight = typeof(PlayerSetup).GetField("_initialAvatarHeight", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo ms_avatarHeight = typeof(PlayerSetup).GetField("_avatarHeight", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static void Init(HarmonyLib.Harmony p_instance)
        {
            // Alternative collider height and radius
            p_instance.Patch(
                typeof(MovementSystem).GetMethod("UpdateCollider", BindingFlags.NonPublic | BindingFlags.Instance),
                new HarmonyLib.HarmonyMethod(typeof(PlayerColliderFix).GetMethod(nameof(OnUpdateCollider_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                null
            );
            p_instance.Patch(
                typeof(PlayerSetup).GetMethod("SetupIKScaling", BindingFlags.NonPublic | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(PlayerColliderFix).GetMethod(nameof(OnSetupIKScaling_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );

            Settings.CollisionScaleChange += OnCollisionScaleChange;
        }

        // Alternative collider size
        static bool OnUpdateCollider_Prefix(
            ref MovementSystem __instance,
            bool __0, // updateRadius
            CharacterController ___controller,
            float ____avatarHeight,
            float ____avatarHeightFactor,
            float ____minimumColliderRadius,
            Vector3 ____colliderCenter
        )
        {
            if(!Settings.CollisionScale)
                return true;

            try
            {
                if(___controller != null)
                {
                    float l_scaledHeight = ____avatarHeight * ____avatarHeightFactor;
                    float l_newRadius = (__0 ? Mathf.Max(____minimumColliderRadius, l_scaledHeight / 6f) : ___controller.radius);

                    float l_newHeight = Mathf.Max(l_scaledHeight, l_newRadius * 2f);
                    float l_currentHeight = ___controller.height;

                    Vector3 l_newCenter = ____colliderCenter;
                    l_newCenter.y = (l_newHeight + 0.075f) * 0.5f; // Idk where 0.075f has come from
                    Vector3 l_currentCenter = ___controller.center;

                    if(__0 || (Mathf.Abs(l_currentHeight - l_newHeight) > (l_currentHeight * 0.05f)) || (Vector3.Distance(l_currentCenter, l_newCenter) > (l_currentHeight * 0.05f)))
                    {
                        if(__0)
                            ___controller.radius = l_newRadius;
                        ___controller.height = l_newHeight;
                        ___controller.center = l_newCenter;

                        __instance.groundDistance = l_newRadius;

                        if(__instance.proxyCollider != null)
                        {
                            if(__0)
                                __instance.proxyCollider.radius = l_newRadius;
                            __instance.proxyCollider.height = l_newHeight;
                            __instance.proxyCollider.center = new Vector3(0f, l_newCenter.y, 0f);
                        }

                        if(__instance.forceObject != null)
                            __instance.forceObject.transform.localScale = new Vector3(l_newRadius + 0.1f, l_newHeight, l_newRadius + 0.1f);
                        if(__instance.groundCheck != null)
                            __instance.groundCheck.localPosition = ____colliderCenter;
                    }
                }
            }
            catch(Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }

            return false;
        }
        static void OnSetupIKScaling_Postfix(
            ref PlayerSetup __instance,
            float ____avatarHeight
        )
        {
            if(!Settings.CollisionScale)
                return;

            try
            {
                __instance._movementSystem.UpdateAvatarHeight(Mathf.Clamp(____avatarHeight, 0.05f, float.MaxValue), true);
            }
            catch(Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }

        static void OnCollisionScaleChange(bool p_state)
        {
            try
            {
                if(p_state)
                    MovementSystem.Instance.UpdateAvatarHeight((float)ms_avatarHeight.GetValue(PlayerSetup.Instance), true);
                else
                    MovementSystem.Instance.UpdateAvatarHeight((float)ms_initialAvatarHeight.GetValue(PlayerSetup.Instance), true);
            }
            catch(Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }
    }
}

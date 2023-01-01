using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Systems.IK.SubSystems;
using ABI_RC.Systems.MovementSystem;
using System.Reflection;
using UnityEngine;

namespace ml_amt
{
    public class AvatarMotionTweaker : MelonLoader.MelonMod
    {
        static readonly MethodInfo[] ms_fbtDetouredMethods =
        {
            typeof(PlayerSetup).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance),
            typeof(PlayerSetup).GetMethod("FixedUpdate", BindingFlags.NonPublic | BindingFlags.Instance),
            typeof(PlayerSetup).GetMethod("UpdatePlayerAvatarMovementData", BindingFlags.NonPublic | BindingFlags.Instance),
            typeof(CVRParameterStreamEntry).GetMethod(nameof(CVRParameterStreamEntry.CheckUpdate))
        };

        static AvatarMotionTweaker ms_instance = null;

        MotionTweaker m_localTweaker = null;

        static bool ms_fbtDetour = false;

        public override void OnInitializeMelon()
        {
            if(ms_instance == null)
                ms_instance = this;

            Settings.Init();

            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnAvatarClear_Postfix), BindingFlags.NonPublic | BindingFlags.Static))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetupAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnSetupAvatar_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(BodySystem).GetMethod(nameof(BodySystem.Calibrate)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnCalibrate_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );

            // FBT detour
            HarmonyInstance.Patch(
                typeof(BodySystem).GetMethod(nameof(BodySystem.FBTAvailable)),
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnFBTAvailable_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                null
            );
            foreach(MethodInfo l_detoured in ms_fbtDetouredMethods)
            {
                HarmonyInstance.Patch(
                    l_detoured,
                    new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(FBTDetour_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                    new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(FBTDetour_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );
            }

            // Alternative collider height
            HarmonyInstance.Patch(
                typeof(MovementSystem).GetMethod("UpdateCollider", BindingFlags.NonPublic | BindingFlags.Instance),
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnUpdateCollider_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                null
            );

            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
        }

        System.Collections.IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            m_localTweaker = PlayerSetup.Instance.gameObject.AddComponent<MotionTweaker>();
            m_localTweaker.SetIKOverrideCrouch(Settings.IKOverrideCrouch);
            m_localTweaker.SetCrouchLimit(Settings.CrouchLimit);
            m_localTweaker.SetIKOverrideCrouch(Settings.IKOverrideProne);
            m_localTweaker.SetProneLimit(Settings.ProneLimit);
            m_localTweaker.SetPoseTransitions(Settings.PoseTransitions);
            m_localTweaker.SetAdjustedMovement(Settings.AdjustedMovement);
            m_localTweaker.SetIKOverrideFly(Settings.IKOverrideFly);
            m_localTweaker.SetIKOverrideJump(Settings.IKOverrideJump);
            m_localTweaker.SetDetectEmotes(Settings.DetectEmotes);
            m_localTweaker.SetFollowHips(Settings.FollowHips);
        }

        public override void OnDeinitializeMelon()
        {
            if(ms_instance == this)
                ms_instance = null;

            m_localTweaker = null;
        }

        static void OnAvatarClear_Postfix() => ms_instance?.OnAvatarClear();
        void OnAvatarClear()
        {
            try
            {
                if(m_localTweaker != null)
                    m_localTweaker.OnAvatarClear();
            }
            catch(System.Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }

        static void OnSetupAvatar_Postfix() => ms_instance?.OnSetupAvatar();
        void OnSetupAvatar()
        {
            try
            {
                if(m_localTweaker != null)
                    m_localTweaker.OnSetupAvatar();
            }
            catch(System.Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }

        static void OnCalibrate_Postfix() => ms_instance?.OnCalibrate();
        void OnCalibrate()
        {
            try
            {
                if(m_localTweaker != null)
                    m_localTweaker.OnCalibrate();
            }
            catch(System.Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }

        // FBT detection override
        static void FBTDetour_Prefix()
        {
            ms_fbtDetour = true;
        }
        static void FBTDetour_Postfix()
        {
            ms_fbtDetour = false;
        }
        static bool OnFBTAvailable_Prefix(ref bool __result)
        {
            if(ms_fbtDetour && !BodySystem.isCalibratedAsFullBody)
            {
                __result = false;
                return false;
            }

            return true;
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
                        bool l_active = ___controller.enabled;

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

                        ___controller.enabled = l_active;
                    }
                }
            }
            catch(System.Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }

            return false;
        }
    }
}

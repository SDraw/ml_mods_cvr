using ABI.CCK.Components;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using System;
using System.Reflection;
using UnityEngine;

namespace ml_pam
{
    public class PickupArmMovement : MelonLoader.MelonMod
    {
        static PickupArmMovement ms_instance = null;

        ArmMover m_localMover = null;

        public override void OnInitializeMelon()
        {
            if(ms_instance == null)
                ms_instance = this;

            Settings.Init();

            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(PickupArmMovement).GetMethod(nameof(OnAvatarClear_Postfix), BindingFlags.NonPublic | BindingFlags.Static))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetupAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(PickupArmMovement).GetMethod(nameof(OnSetupAvatar_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(CVRPickupObject).GetMethod(nameof(CVRPickupObject.Grab)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(PickupArmMovement).GetMethod(nameof(OnCVRPickupObjectGrab_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(CVRPickupObject).GetMethod(nameof(CVRPickupObject.Drop)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(PickupArmMovement).GetMethod(nameof(OnCVRPickupObjectDrop_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod("SetPlaySpaceScale", BindingFlags.NonPublic | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(PickupArmMovement).GetMethod(nameof(OnPlayspaceScale_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );

            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
        }

        System.Collections.IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            m_localMover = PlayerSetup.Instance.gameObject.AddComponent<ArmMover>();
        }

        public override void OnDeinitializeMelon()
        {
            if(ms_instance == this)
                ms_instance = null;
        }

        static void OnAvatarClear_Postfix() => ms_instance?.OnAvatarClear();
        void OnAvatarClear()
        {
            try
            {
                if(m_localMover != null)
                    m_localMover.OnAvatarClear();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnSetupAvatar_Postfix() => ms_instance?.OnSetupAvatar();
        void OnSetupAvatar()
        {
            try
            {
                if(m_localMover != null)
                    m_localMover.OnAvatarSetup();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnCVRPickupObjectGrab_Postfix(ref CVRPickupObject __instance, ControllerRay __1, Vector3 __2) => ms_instance?.OnCVRPickupObjectGrab(__instance, __1, __2);
        void OnCVRPickupObjectGrab(CVRPickupObject p_pickup, ControllerRay p_ray, Vector3 p_hit)
        {
            try
            {
                if(p_pickup.IsGrabbedByMe() && (m_localMover != null))
                    m_localMover.OnPickupGrab(p_pickup, p_ray, p_hit);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnCVRPickupObjectDrop_Postfix(ref CVRPickupObject __instance) => ms_instance?.OnCVRPickupObjectDrop(__instance);
        void OnCVRPickupObjectDrop(CVRPickupObject p_pickup)
        {
            try
            {
                if(m_localMover != null)
                    m_localMover.OnPickupDrop(p_pickup);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnPlayspaceScale_Postfix(float ____avatarScaleRelation) => ms_instance?.OnPlayspaceScale(____avatarScaleRelation);
        void OnPlayspaceScale(float p_relation)
        {
            try
            {
                if(m_localMover != null)
                    m_localMover.OnPlayspaceScale(p_relation);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

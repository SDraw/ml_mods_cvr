using ABI.CCK.Components;
using ABI_RC.Core.Player;
using System;
using System.Reflection;
using UnityEngine;

namespace ml_pam
{
    public class PickupArmMovement : MelonLoader.MelonMod
    {
        static PickupArmMovement ms_instance = null;

        ArmMover m_localPuller = null;

        public override void OnInitializeMelon()
        {
            if(ms_instance == null)
                ms_instance = this;

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
                m_localPuller = null;
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
                if(!Utils.IsInVR())
                    m_localPuller = PlayerSetup.Instance._avatar.AddComponent<ArmMover>();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnCVRPickupObjectGrab_Postfix(ref CVRPickupObject __instance, Vector3 __2) => ms_instance?.OnCVRPickupObjectGrab(__instance, __2);
        void OnCVRPickupObjectGrab(CVRPickupObject p_pickup, Vector3 p_hit)
        {
            try
            {
                if(p_pickup.IsGrabbedByMe() && (m_localPuller != null))
                {
                    m_localPuller.SetTarget(p_pickup, p_hit);
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnCVRPickupObjectDrop_Postfix() => ms_instance?.OnCVRPickupObjectDrop();
        void OnCVRPickupObjectDrop()
        {
            try
            {
                if(m_localPuller != null)
                {
                    m_localPuller.SetTarget(null, Vector3.zero);
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

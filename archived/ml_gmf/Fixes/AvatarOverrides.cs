using ABI.CCK.Components;
using ABI_RC.Core.Player;
using System;
using System.Reflection;

namespace ml_gmf.Fixes
{
    static class AvatarOverrides
    {
        internal static void Init(HarmonyLib.Harmony p_instance)
        {
            p_instance.Patch(
                typeof(PlayerSetup).GetMethod("SetupAvatarGeneral", BindingFlags.NonPublic | BindingFlags.Instance),
                new HarmonyLib.HarmonyMethod(typeof(AvatarOverrides).GetMethod(nameof(OnSetupAvatarGeneral_Prefix), BindingFlags.NonPublic | BindingFlags.Static))
            );
            p_instance.Patch(
                typeof(PuppetMaster).GetMethod(nameof(PuppetMaster.AvatarInstantiated), BindingFlags.Public | BindingFlags.Instance),
                new HarmonyLib.HarmonyMethod(typeof(AvatarOverrides).GetMethod(nameof(OnPuppetAvatarInstantiated_Prefix), BindingFlags.NonPublic | BindingFlags.Static))
            );
        }

        static void OnSetupAvatarGeneral_Prefix(CVRAvatar ____avatarDescriptor)
        {
            try
            {
                if(____avatarDescriptor.overrides != null)
                    ____avatarDescriptor.overrides = UnityEngine.Object.Instantiate(____avatarDescriptor.overrides);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
        static void OnPuppetAvatarInstantiated_Prefix(ref PuppetMaster __instance)
        {
            try
            {
                CVRAvatar l_avatar = __instance.avatarObject.GetComponent<CVRAvatar>();
                if((l_avatar != null) && (l_avatar.overrides != null))
                    l_avatar.overrides = UnityEngine.Object.Instantiate(l_avatar.overrides);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

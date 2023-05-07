using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Systems.IK.SubSystems;
using System.Reflection;

namespace ml_amt.Fixes
{
    static class FBTDetectionFix
    {
        static readonly MethodInfo[] ms_fbtDetouredMethods =
        {
            typeof(PlayerSetup).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance),
            typeof(PlayerSetup).GetMethod("FixedUpdate", BindingFlags.NonPublic | BindingFlags.Instance),
            typeof(PlayerSetup).GetMethod("UpdatePlayerAvatarMovementData", BindingFlags.NonPublic | BindingFlags.Instance),
            typeof(CVRParameterStreamEntry).GetMethod(nameof(CVRParameterStreamEntry.CheckUpdate))
        };

        static bool ms_fbtDetour = false;

        internal static void Init(HarmonyLib.Harmony p_instance)
        {
            // FBT detour
            p_instance.Patch(
                typeof(BodySystem).GetMethod(nameof(BodySystem.FBTAvailable)),
                new HarmonyLib.HarmonyMethod(typeof(FBTDetectionFix).GetMethod(nameof(OnFBTAvailable_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                null
            );
            foreach(MethodInfo l_detoured in ms_fbtDetouredMethods)
            {
                p_instance.Patch(
                    l_detoured,
                    new HarmonyLib.HarmonyMethod(typeof(FBTDetectionFix).GetMethod(nameof(FBTDetour_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                    new HarmonyLib.HarmonyMethod(typeof(FBTDetectionFix).GetMethod(nameof(FBTDetour_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );
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
    }
}

using ABI_RC.Core.UI;
using ABI_RC.Core.EventSystem;

namespace ml_aci
{
    public class AvatarChangeInfo : MelonLoader.MelonMod
    {
        public override void OnApplicationStart()
        {
            HarmonyInstance.Patch(
                typeof(AssetManagement).GetMethod(nameof(AssetManagement.LoadLocalAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarChangeInfo).GetMethod(nameof(OnLocalAvatarLoad), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static))
            );
        }

        static void OnLocalAvatarLoad()
        {
            if(CohtmlHud.Instance != null)
                CohtmlHud.Instance.ViewDropText("Avatar changed", "Please, wait ...");
        }
    }
}

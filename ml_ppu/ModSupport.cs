using System.Linq;

namespace ml_ppu
{
    static class ModSupport
    {
        static bool ms_ragdollPresent = false;

        internal static void Init()
        {
            ms_ragdollPresent = (MelonLoader.MelonBase.RegisteredMelons.FirstOrDefault(m => m.Info.Name == "PlayerRagdollMod") != null);
        }

        public static bool IsRagdolled() => (ms_ragdollPresent && IsRagdollInternal());
        static bool IsRagdollInternal() => ml_prm.RagdollController.Instance.IsRagdolled();

        public static void TryToUnragdoll()
        {
            if(ms_ragdollPresent)
                TryToUngradollInternal();
        }
        static void TryToUngradollInternal() => ml_prm.RagdollController.Instance.Unragdoll();
    }
}

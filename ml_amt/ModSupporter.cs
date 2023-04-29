using System.Collections;
using System.Linq;

namespace ml_amt
{
    static class ModSupporter
    {
        static bool ms_ragdollMod = false;
        static bool ms_copycatMod = false;

        public static void Init()
        {
            if(MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m.Info.Name == "PlayerRagdollMod") != null)
                MelonLoader.MelonCoroutines.Start(WaitForRagdollInstance());
            if(MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m.Info.Name == "PlayerMovementCopycat") != null)
                MelonLoader.MelonCoroutines.Start(WaitForCopycatInstance());
        }

        // PlayerRagdollMod support
        static IEnumerator WaitForRagdollInstance()
        {
            while(ml_prm.RagdollController.Instance == null)
                yield return null;

            ms_ragdollMod = true;
        }
        static bool IsRagdolled() => ml_prm.RagdollController.Instance.IsRagdolled();

        // PlayerMovementCopycat support
        static IEnumerator WaitForCopycatInstance()
        {
            while(ml_pmc.PoseCopycat.Instance == null)
                yield return null;

            ms_copycatMod = true;
        }
        static bool IsCopycating() => ml_pmc.PoseCopycat.Instance.IsActive();

        public static bool SkipHipsOverride()
        {
            bool l_result = false;
            l_result |= (ms_ragdollMod && IsRagdolled());
            l_result |= (ms_copycatMod && IsCopycating());
            return l_result;
        }


    }
}

using System.Collections;
using System.Linq;

namespace ml_amt
{
    static class ModSupporter
    {
        static bool ms_ragdollMod = false;

        public static void Init()
        {
            if(MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m.Info.Name == "PlayerRagdollMod") != null)
                MelonLoader.MelonCoroutines.Start(WaitForRagdollInstance());
        }

        // PlayerRagdollMod support
        static IEnumerator WaitForRagdollInstance()
        {
            while(ml_prm.RagdollController.Instance == null)
                yield return null;

            ms_ragdollMod = true;
        }
        static bool IsRagdolled() => ml_prm.RagdollController.Instance.IsRagdolled();
        public static bool SkipHipsOverride()
        {
            bool l_result = false;
            l_result |= (ms_ragdollMod && IsRagdolled());
            return l_result;
        }
    }
}

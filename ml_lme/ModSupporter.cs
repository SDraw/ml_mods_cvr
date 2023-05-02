using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ml_lme
{
    static class ModSupporter
    {
        static bool ms_copycatMod = false;

        public static void Init()
        {
            if(MelonLoader.MelonMod.RegisteredMelons.FirstOrDefault(m => m.Info.Name == "PlayerMovementCopycat") != null)
                MelonLoader.MelonCoroutines.Start(WaitForCopycatInstance());
        }

        // PlayerMovementCopycat support
        static IEnumerator WaitForCopycatInstance()
        {
            while(ml_pmc.PoseCopycat.Instance == null)
                yield return null;

            ms_copycatMod = true;
        }
        static bool IsCopycating() => (ml_pmc.PoseCopycat.Instance.IsActive() && ml_pmc.PoseCopycat.Instance.IsFingerTrackingActive());

        public static bool SkipFingersOverride()
        {
            bool l_result = false;
            l_result |= (ms_copycatMod && IsCopycating());
            return l_result;
        }
    }
}

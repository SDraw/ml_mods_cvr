using ABI_RC.Core.Player;
using UnityEngine;

namespace ml_pmc
{
    public class PlayerMovementCopycat : MelonLoader.MelonMod
    {
        PoseCopycat m_poseCopycat = null;

        public override void OnInitializeMelon()
        {
            Settings.Init();
            GameEvents.Init(HarmonyInstance);
        }

        public override void OnLateInitializeMelon()
        {
            ModUi.Init();
            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
        }

        System.Collections.IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            m_poseCopycat = new GameObject("[PlayerMovementCopycat]").AddComponent<PoseCopycat>();
        }

        public override void OnDeinitializeMelon()
        {
            if(m_poseCopycat != null)
                Object.Destroy(m_poseCopycat.gameObject);
            m_poseCopycat = null;
        }
    }
}

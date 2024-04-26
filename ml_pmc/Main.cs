using ABI_RC.Core.Player;

namespace ml_pmc
{
    public class PlayerMovementCopycat : MelonLoader.MelonMod
    {
        PoseCopycat m_localCopycat = null;

        public override void OnInitializeMelon()
        {
            Settings.Init();
            ModUi.Init();
            GameEvents.Init(HarmonyInstance);

            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
        }

        public override void OnDeinitializeMelon()
        {
            if(m_localCopycat != null)
                UnityEngine.Object.Destroy(m_localCopycat);
            m_localCopycat = null;
        }

        System.Collections.IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            m_localCopycat = PlayerSetup.Instance.gameObject.AddComponent<PoseCopycat>();
        }
    }
}

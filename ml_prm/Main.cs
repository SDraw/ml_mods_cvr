using ABI_RC.Core.Util.AssetFiltering;

namespace ml_prm
{
    public class PlayerRagdollMod : MelonLoader.MelonMod
    {
        RagdollController m_controller = null;
        SoundManager m_soundManager = null;

        public override void OnInitializeMelon()
        {
            Settings.Init();
            GameEvents.Init(HarmonyInstance);
            WorldManager.Init();
            ResourcesHandler.ExtractResources();
        }

        public override void OnLateInitializeMelon()
        {
            ModUi.Init();
            MelonLoader.MelonCoroutines.Start(WaitForRootLogic());
            MelonLoader.MelonCoroutines.Start(WaitForWhitelist());
        }

        System.Collections.IEnumerator WaitForRootLogic()
        {
            while(ABI_RC.Core.RootLogic.Instance == null)
                yield return null;

            m_controller = new UnityEngine.GameObject("[PlayerRagdollMod]").AddComponent<RagdollController>();
            m_soundManager = new SoundManager(m_controller.transform);
            m_soundManager.LoadSounds();
        }

        System.Collections.IEnumerator WaitForWhitelist()
        {
            // Whitelist the toggle script
            while(SharedFilter.LocalComponentWhitelist == null)
                yield return null;
            SharedFilter.LocalComponentWhitelist.Add(typeof(RagdollToggle));
        }

        public override void OnDeinitializeMelon()
        {
            WorldManager.DeInit();

            m_soundManager = null;

            if(m_controller != null)
                UnityEngine.Object.Destroy(m_controller.gameObject);
            m_controller = null;
        }
    }
}

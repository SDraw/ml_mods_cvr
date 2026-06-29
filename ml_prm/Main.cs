using ABI_RC.Core.Util.AssetFiltering;
using System;
using System.Collections;

namespace ml_prm
{
    public class PlayerRagdollMod : MelonLoader.MelonMod
    {
        public static readonly Guid ms_modGuid = new Guid("19128384-2f31-4a86-bcbd-6fa889dcc2ad");

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

        IEnumerator WaitForRootLogic()
        {
            while(ABI_RC.Core.RootLogic.Instance == null)
                yield return null;

            m_controller = new UnityEngine.GameObject("[PlayerRagdollMod]").AddComponent<RagdollController>();
            m_soundManager = new SoundManager(m_controller.transform);
            m_soundManager.LoadSounds();
        }

        IEnumerator WaitForWhitelist()
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

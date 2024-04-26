using ABI_RC.Core.Player;
using ABI_RC.Core.Util.AssetFiltering;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ml_prm
{
    public class PlayerRagdollMod : MelonLoader.MelonMod
    {
        RagdollController m_localController = null;

        public override void OnInitializeMelon()
        {
            Settings.Init();
            ModUi.Init();
            GameEvents.Init(HarmonyInstance);

            // Whitelist the toggle script
            (typeof(SharedFilter).GetField("_localComponentWhitelist", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as HashSet<Type>)?.Add(typeof(RagdollToggle));

            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
        }

        public override void OnDeinitializeMelon()
        {
            if(m_localController != null)
                UnityEngine.Object.Destroy(m_localController);
            m_localController = null;
        }

        System.Collections.IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            m_localController = PlayerSetup.Instance.gameObject.AddComponent<RagdollController>();
        }
    }
}

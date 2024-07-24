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
            WorldHandler.Init();

            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
            MelonLoader.MelonCoroutines.Start(WaitForWhitelist());
        }

        public override void OnDeinitializeMelon()
        {
            WorldHandler.DeInit();

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

        System.Collections.IEnumerator WaitForWhitelist()
        {
            // Whitelist the toggle script
            FieldInfo l_field = typeof(SharedFilter).GetField("_localComponentWhitelist", BindingFlags.NonPublic | BindingFlags.Static);
            HashSet<Type> l_hashSet = l_field?.GetValue(null) as HashSet<Type>;
            while(l_hashSet == null)
            {
                l_hashSet = l_field?.GetValue(null) as HashSet<Type>;
                yield return null;
            }
            l_hashSet.Add(typeof(RagdollToggle));
        }
    }
}

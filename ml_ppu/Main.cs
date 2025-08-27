using System;
using System.Collections;
using UnityEngine;

namespace ml_ppu
{
    public class PlayerPickUp : MelonLoader.MelonMod
    {
        PickUpManager m_manager = null;

        public override void OnInitializeMelon()
        {
            Settings.Init();
            GameEvents.Init(HarmonyInstance);
            ModSupport.Init();
        }

        public override void OnLateInitializeMelon()
        {
            ModUi.Init();
            MelonLoader.MelonCoroutines.Start(WaitForRootLogic());
        }

        IEnumerator WaitForRootLogic()
        {
            while(ABI_RC.Core.RootLogic.Instance == null)
                yield return null;

            m_manager = new GameObject("[PlayerPickUp]").AddComponent<PickUpManager>();
        }

        public override void OnDeinitializeMelon()
        {
            if(m_manager != null)
            {
                UnityEngine.Object.Destroy(m_manager.gameObject);
                m_manager = null;
            }
        }
    }
}

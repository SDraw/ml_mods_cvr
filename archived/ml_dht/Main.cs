using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using UnityEngine;

namespace ml_dht
{
    public class DesktopHeadTracking : MelonLoader.MelonMod
    {
        
        HeadTracked m_tracked = null;

        public override void OnInitializeMelon()
        {
            Settings.Init();
            GameEvents.InitA(HarmonyInstance);

            MelonLoader.MelonCoroutines.Start(WaitForInstances());
        }

        System.Collections.IEnumerator WaitForInstances()
        {
            while(MetaPort.Instance == null)
                yield return null;

            while(PlayerSetup.Instance == null)
                yield return null;

            GameEvents.InitB(HarmonyInstance);

            m_tracked = new GameObject("[DesktopHeadTracking]").AddComponent<HeadTracked>();
        }

        public override void OnDeinitializeMelon()
        {
            if(m_tracked != null)
                Object.Destroy(m_tracked.gameObject);
            m_tracked = null;
        }
    }
}
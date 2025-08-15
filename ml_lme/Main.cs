using System.Collections;
using UnityEngine;

namespace ml_lme
{

    public class LeapMotionExtension : MelonLoader.MelonMod
    {
        LeapManager m_leapManager = null;

        public override void OnInitializeMelon()
        {
            DependenciesHandler.ExtractDependencies();
            Settings.Init();
            AssetsHandler.Load();
            GameEvents.Init(HarmonyInstance);

            MelonLoader.MelonCoroutines.Start(WaitForRootLogic());
        }

        IEnumerator WaitForRootLogic()
        {
            while(ABI_RC.Core.RootLogic.Instance == null)
                yield return null;

            m_leapManager = new GameObject("[LeapMotionExtension]").AddComponent<LeapManager>();
        }

        public override void OnDeinitializeMelon()
        {
            if(m_leapManager != null)
                Object.Destroy(m_leapManager);
            m_leapManager = null;
        }
    }
}

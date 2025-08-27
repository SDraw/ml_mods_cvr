using System.Collections;
using UnityEngine;

namespace ml_amt
{
    public class AvatarMotionTweaker : MelonLoader.MelonMod
    {
        MotionTweaker m_tweaker = null;

        public override void OnInitializeMelon()
        {
            GameEvents.Init(HarmonyInstance);
        }

        public override void OnLateInitializeMelon()
        {
            Settings.Init();
            MelonLoader.MelonCoroutines.Start(WaitForRootLogic());
        }

        IEnumerator WaitForRootLogic()
        {
            while(ABI_RC.Core.RootLogic.Instance == null)
                yield return null;

            m_tweaker = new GameObject("[AvatarMotionTweaker]").AddComponent<MotionTweaker>();
        }

        public override void OnDeinitializeMelon()
        {
            if(m_tweaker != null)
                Object.Destroy(m_tweaker.gameObject);
            m_tweaker = null;
        }
    }
}

using ABI_RC.Core.Player;
using System.Collections;

namespace ml_amt
{
    public class AvatarMotionTweaker : MelonLoader.MelonMod
    {
        MotionTweaker m_localTweaker = null;

        public override void OnInitializeMelon()
        {
            Settings.Init();
            GameEvents.Init(HarmonyInstance);

            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
        }

        IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            m_localTweaker = PlayerSetup.Instance.gameObject.AddComponent<MotionTweaker>();
        }

        public override void OnDeinitializeMelon()
        {
            if(m_localTweaker != null)
                UnityEngine.Object.Destroy(m_localTweaker);
            m_localTweaker = null;
        }
    }
}

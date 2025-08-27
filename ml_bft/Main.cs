using System.Collections;

namespace ml_bft
{
    public class BetterFingersTracking : MelonLoader.MelonMod
    {
        InputHandler m_inputHandler = null;
        FingerSystem m_fingerSystem = null;

        public override void OnInitializeMelon()
        {
            AssetsHandler.Load();
            GameEvents.Init(HarmonyInstance);
        }

        public override void OnLateInitializeMelon()
        {
            Settings.Init();
            MelonLoader.MelonCoroutines.Start(WaitForInstances());
        }

        IEnumerator WaitForInstances()
        {
            while(ABI_RC.Systems.InputManagement.CVRInputManager.Instance == null)
                yield return null;

            m_inputHandler = new InputHandler();
            m_fingerSystem = new FingerSystem();
        }

        public override void OnDeinitializeMelon()
        {
            m_inputHandler?.Cleanup();
            m_inputHandler = null;

            m_fingerSystem?.Cleanup();
            m_fingerSystem = null;
        }
    }
}

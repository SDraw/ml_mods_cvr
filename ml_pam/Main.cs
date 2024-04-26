using ABI_RC.Core.Player;

namespace ml_pam
{
    public class PickupArmMovement : MelonLoader.MelonMod
    {
        ArmMover m_localMover = null;

        public override void OnInitializeMelon()
        {
            Settings.Init();
            GameEvents.Init(HarmonyInstance);

            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
        }

        System.Collections.IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            m_localMover = PlayerSetup.Instance.gameObject.AddComponent<ArmMover>();
        }

        public override void OnDeinitializeMelon()
        {
            if(m_localMover != null)
                UnityEngine.Object.Destroy(m_localMover);
            m_localMover = null;
        }
    }
}

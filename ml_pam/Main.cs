using UnityEngine;

namespace ml_pam
{
    public class PickupArmMovement : MelonLoader.MelonMod
    {
        ArmMover m_mover = null;

        public override void OnInitializeMelon()
        {
            Settings.Init();
            GameEvents.Init(HarmonyInstance);

            MelonLoader.MelonCoroutines.Start(WaitForRootLogic());
        }

        System.Collections.IEnumerator WaitForRootLogic()
        {
            while(ABI_RC.Core.RootLogic.Instance == null)
                yield return null;
            while(ABI_RC.Core.Player.PlayerSetup.Instance == null)
                yield return null;

            m_mover = new GameObject("[PickupArmMovement]").AddComponent<ArmMover>();
        }

        public override void OnDeinitializeMelon()
        {
            if(m_mover != null)
                Object.Destroy(m_mover.gameObject);
            m_mover = null;
        }
    }
}

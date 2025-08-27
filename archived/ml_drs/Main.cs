using ABI_RC.Core.UI;
using UnityEngine;

namespace ml_drs
{
    public class DesktopReticleSwitch : MelonLoader.MelonMod
    {
        public override void OnUpdate()
        {
            if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            {
                if((CohtmlHud.Instance != null) && (CohtmlHud.Instance.desktopPointer != null))
                    CohtmlHud.Instance.desktopPointer.SetActive(!CohtmlHud.Instance.desktopPointer.activeSelf);
            }
        }
    }
}

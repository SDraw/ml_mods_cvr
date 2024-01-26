using ABI_RC.Core.Player;
using System.Collections;

namespace ml_gmf.Fixes
{
    static class PostProccesVolumes
    {
        internal static void Init()
        {
            MelonLoader.MelonCoroutines.Start(FixVRCameraVolumeTarget());
        }

        static IEnumerator FixVRCameraVolumeTarget()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            while(PlayerSetup.Instance.vrCamera == null)
                yield return null;

            UnityEngine.Rendering.PostProcessing.PostProcessLayer l_layer = null;
            while(l_layer == null)
            {
                l_layer = PlayerSetup.Instance.vrCamera.GetComponent<UnityEngine.Rendering.PostProcessing.PostProcessLayer>();
                yield return null;
            }

            l_layer.volumeTrigger = PlayerSetup.Instance.vrCamera.transform;
        }
    }
}

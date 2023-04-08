using ABI.CCK.Components;
using UnityEngine;

namespace ml_prm
{
    static class Utils
    {
        public static bool IsInVR() => ((ABI_RC.Core.Savior.CheckVR.Instance != null) && ABI_RC.Core.Savior.CheckVR.Instance.hasVrDeviceLoaded);
        public static bool IsWorldSafe() => ((CVRWorld.Instance != null) && CVRWorld.Instance.allowFlying);
        public static float GetWorldFlyMultiplier()
        {
            float l_result = 1f;
            if(CVRWorld.Instance != null)
                l_result = CVRWorld.Instance.flyMultiplier;
            return l_result;
        }

        public static void CopyGlobal(this Transform p_source, Transform p_target)
        {
            p_target.position = p_source.position;
            p_target.rotation = p_source.rotation;
        }
    }
}

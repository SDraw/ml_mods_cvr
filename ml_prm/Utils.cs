using UnityEngine;

namespace ml_prm
{
    static class Utils
    {
        public static void CopyGlobal(this Transform p_source, Transform p_target)
        {
            p_target.position = p_source.position;
            p_target.rotation = p_source.rotation;
        }
    }
}

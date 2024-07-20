using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Core.UI;
using ABI_RC.Systems.IK;
using System.Reflection;
using UnityEngine;

namespace ml_dht
{
    static class Utils
    {
        static readonly object[] ms_emptyArray = new object[0];
        static readonly FieldInfo ms_view = typeof(CohtmlControlledViewWrapper).GetField("_view", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo ms_updateShapesLocal = typeof(CVRFaceTracking).GetMethod("UpdateShapesLocal", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void ExecuteScript(this CohtmlControlledViewWrapper p_instance, string p_script) => ((cohtml.Net.View)ms_view.GetValue(p_instance)).ExecuteScript(p_script);

        public static void UpdateShapesLocal_Private(this CVRFaceTracking p_instance) => ms_updateShapesLocal?.Invoke(p_instance, ms_emptyArray);

        public static void SetAvatarTPose()
        {
            if(PlayerSetup.Instance._animator.isHuman)
            {
                IKSystem.Instance.SetAvatarPose(IKSystem.AvatarPose.TPose);
                PlayerSetup.Instance._avatar.transform.localPosition = Vector3.zero;
                PlayerSetup.Instance._avatar.transform.localRotation = Quaternion.identity;
            }
        }
    }
}

using ABI.CCK.Components;
using ABI_RC.Core.Savior;
using ABI_RC.Core.UI;
using System.Reflection;

namespace ml_dht
{
    static class Utils
    {
        static readonly object[] ms_emptyArray = new object[0];
        static readonly FieldInfo ms_view = typeof(CohtmlControlledViewWrapper).GetField("_view", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly MethodInfo ms_updateShapesLocal = typeof(CVRFaceTracking).GetMethod("UpdateShapesLocal", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool IsInVR() => ((MetaPort.Instance != null) && MetaPort.Instance.isUsingVr);

        public static void ExecuteScript(this CohtmlControlledViewWrapper p_instance, string p_script) => (ms_view?.GetValue(p_instance) as cohtml.Net.View)?.ExecuteScript(p_script);

        public static void UpdateShapesLocal_Private(this CVRFaceTracking p_instance) => ms_updateShapesLocal?.Invoke(p_instance, ms_emptyArray);
    }
}

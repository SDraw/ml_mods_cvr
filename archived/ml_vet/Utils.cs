using ABI_RC.Core.Savior;
using ABI_RC.Core.UI;
using System.Reflection;

namespace ml_vet
{
    public static class Utils
    {
        static readonly FieldInfo ms_view = typeof(CohtmlControlledViewWrapper).GetField("_view", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void ExecuteScript(this CohtmlControlledViewWrapper p_instance, string p_script) => (ms_view?.GetValue(p_instance) as cohtml.Net.View)?.ExecuteScript(p_script);

        public static bool IsInVR() => ((MetaPort.Instance != null) && MetaPort.Instance.isUsingVr);
    }
}

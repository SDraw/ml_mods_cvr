using ABI_RC.Core.UI;
using System.Reflection;

namespace ml_asl
{
    static class Utils
    {
        static readonly FieldInfo ms_view = typeof(CohtmlControlledViewWrapper).GetField("_view", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void ExecuteScript(this CohtmlControlledViewWrapper p_instance, string p_script) => ((cohtml.Net.View)ms_view.GetValue(p_instance)).ExecuteScript(p_script);
    }
}

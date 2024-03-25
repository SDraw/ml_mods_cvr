using ABI_RC.Core.UI;
using ABI_RC.Systems.InputManagement;
using System.Reflection;

namespace ml_bft
{
    static class Utils
    {
        static readonly FieldInfo ms_view = typeof(CohtmlControlledViewWrapper).GetField("_view", BindingFlags.NonPublic | BindingFlags.Instance);

        static public void ExecuteScript(this CohtmlControlledViewWrapper p_instance, string p_script) => ((cohtml.Net.View)ms_view.GetValue(p_instance)).ExecuteScript(p_script);

        public static bool AreKnucklesInUse() => ((CVRInputManager.Instance._leftController == ABI_RC.Systems.InputManagement.XR.eXRControllerType.Index) || (CVRInputManager.Instance._rightController == ABI_RC.Systems.InputManagement.XR.eXRControllerType.Index));
    }
}

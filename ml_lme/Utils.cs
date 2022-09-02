using ABI_RC.Core.Player;
using System.Linq;

namespace ml_lme
{
    static class Utils
    {
        public static bool AreKnucklesInUse() => PlayerSetup.Instance._trackerManager.trackerNames.Contains("knuckles");

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }
}

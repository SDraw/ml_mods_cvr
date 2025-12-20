using ABI_RC.Core.Networking.IO.Instancing;
using ABI_RC.Core.UI;
using System.Reflection;

namespace ml_pin
{
    static class Utils
    {
        static readonly FieldInfo ms_view = typeof(CohtmlControlledViewWrapper).GetField("_view", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void ExecuteScript(this CohtmlControlledViewWrapper p_instance, string p_script) => (ms_view?.GetValue(p_instance) as cohtml.Net.View)?.ExecuteScript(p_script);

        // Instance info
        public static bool IsInPublicInstance()
        {
            bool l_result = false;
            switch(Instances.CurrentInstancePrivacyType)
            {
                case Instances.InstancePrivacyType.Public:
                case Instances.InstancePrivacyType.GroupPublic:
                    l_result = true;
                    break;
            }
            return l_result;
        }

        public static bool IsInFriendsInstance()
        {
            bool l_result = false;
            switch(Instances.CurrentInstancePrivacyType)
            {
                case Instances.InstancePrivacyType.Friends:
                case Instances.InstancePrivacyType.FriendsOfFriends:
                case Instances.InstancePrivacyType.GroupPlus:
                    l_result = true;
                    break;
            }
            return l_result;
        }

        public static bool IsInPrivateInstance()
        {
            bool l_result = false;
            switch(Instances.CurrentInstancePrivacyType)
            {
                case Instances.InstancePrivacyType.EveryoneCanInvite:
                case Instances.InstancePrivacyType.OwnerMustInvite:
                case Instances.InstancePrivacyType.Group:
                    l_result = true;
                    break;
            }
            return l_result;
        }
    }
}

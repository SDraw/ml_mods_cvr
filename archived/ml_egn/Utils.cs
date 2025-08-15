using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Networking;
using ABI_RC.Core.Savior;
using ABI_RC.Core.UI;
using DarkRift;

namespace ml_egn
{
    static class Utils
    {
        public static bool IsMenuOpened()
        {
            return ((ViewManager.Instance != null) ? ViewManager.Instance.isGameMenuOpen() : false);
        }

        public static void ShowMenuNotification(string p_message, float p_time = 1f)
        {
            if(ViewManager.Instance != null)
                ViewManager.Instance.TriggerPushNotification(p_message, p_time);
        }

        public static void ShowMenuAlert(string p_title, string p_message)
        {
            if(ViewManager.Instance != null)
                ViewManager.Instance.TriggerAlert(p_title, p_message, -1, true);
        }

        public static void ShowHUDNotification(string p_title, string p_message, string p_small = "", bool p_immediate = false)
        {
            if(CohtmlHud.Instance != null)
            {
                if(p_immediate)
                    CohtmlHud.Instance.ViewDropTextImmediate(p_title, p_message, p_small);
                else
                    CohtmlHud.Instance.ViewDropText(p_title, p_message, p_small);
            }
        }

        public static bool IsConnected()
        {
            bool l_result = false;
            if((NetworkManager.Instance != null) && (NetworkManager.Instance.GameNetwork != null))
                l_result = (NetworkManager.Instance.GameNetwork.ConnectionState == ConnectionState.Connected);
            return l_result;
        }

        public static bool ArePropsAllowed() => ((MetaPort.Instance != null) && MetaPort.Instance.worldAllowProps);
        public static bool ArePropsEnabled() => ((MetaPort.Instance != null) && MetaPort.Instance.settings.GetSettingsBool("ContentFilterPropsEnabled"));
    }
}

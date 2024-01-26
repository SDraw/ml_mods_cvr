using ABI_RC.Core.UI;
using DarkRift.Client;
using System.Reflection;

namespace ml_sci
{
    public class ServerConnectionInfo : MelonLoader.MelonMod
    {
        public override void OnInitializeMelon()
        {
            HarmonyInstance.Patch(
                typeof(ABI_RC.Core.Networking.NetworkManager).GetMethod("OnGameNetworkConnectionClosed", BindingFlags.NonPublic | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(ServerConnectionInfo).GetMethod(nameof(OnGameNetworkConnectionClosed), BindingFlags.NonPublic | BindingFlags.Static))
            );
        }

        static void OnGameNetworkConnectionClosed(object __0, DisconnectedEventArgs __1)
        {
            try
            {
                if((CohtmlHud.Instance != null) && (__1 != null) && (!__1.LocalDisconnect))
                    CohtmlHud.Instance.ViewDropTextImmediate("(Local) Client", "Connection lost", (__1.Error != System.Net.Sockets.SocketError.Success) ? ("Reason: " + __1.Error.ToString()) : "");
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}

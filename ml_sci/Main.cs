using ABI_RC.Core.UI;
using DarkRift.Client;

namespace ml_sci
{
    public class ServerConnectionInfo : MelonLoader.MelonMod
    {
        public override void OnApplicationStart()
        {
            HarmonyInstance.Patch(
                typeof(ABI_RC.Core.Networking.NetworkManager).GetMethod("OnGameNetworkConnectionClosed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(ServerConnectionInfo).GetMethod(nameof(OnGameNetworkConnectionClosed), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static))
            );
        }

        static void OnGameNetworkConnectionClosed(object __0, DisconnectedEventArgs __1)
        {
            if((CohtmlHud.Instance != null) && (__1 != null) && (!__1.LocalDisconnect))
                CohtmlHud.Instance.ViewDropTextImmediate("(Local) Client", "Connection lost", (__1.Error != System.Net.Sockets.SocketError.Success) ? ("Reason: " + __1.Error.ToString()) : "");
        }
    }
}

using ABI_RC.VideoPlayer;
using System.Reflection;
using System.IO;

namespace ml_vpc
{
    public class VideoPlayerCookies : MelonLoader.MelonMod
    {
        static string ms_cookiesPath;

        public override void OnInitializeMelon()
        {
            HarmonyInstance.Patch(typeof(YoutubeDl).GetMethod("GetVideoMetaDataAsync", BindingFlags.NonPublic | BindingFlags.Static),
                new HarmonyLib.HarmonyMethod(typeof(VideoPlayerCookies).GetMethod(nameof(OnGetYoutubeVideoMetaData_Prefix), BindingFlags.NonPublic | BindingFlags.Static))
            );

            ms_cookiesPath = Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, "cookies.txt");
        }

        static void OnGetYoutubeVideoMetaData_Prefix(ref string parameter)
        {
            if(File.Exists(ms_cookiesPath))
                parameter += string.Format(" --cookies \"{0}\"", ms_cookiesPath);
        }
    }
}

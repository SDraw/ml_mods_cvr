using System;
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
            HarmonyInstance.Patch(typeof(YoutubeDl).GetMethod("GetVideoMetaDataAsync", BindingFlags.Public | BindingFlags.Static),
                new HarmonyLib.HarmonyMethod(typeof(VideoPlayerCookies).GetMethod(nameof(OnGetYoutubeVideoMetaData_Prefix), BindingFlags.NonPublic | BindingFlags.Static))
            );

            ms_cookiesPath = Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, "cookies.txt");
        }

        public override void OnLateInitializeMelon()
        {
            Settings.Init();
        }

        static void OnGetYoutubeVideoMetaData_Prefix(ref string youtubeUrl, ref string existingParameters)
        {
            try
            {
                if (!Settings.Enabled)
                    return;

                switch (Settings.Mode)
                {
                    case Settings.CookieMode.File:
                        if (File.Exists(ms_cookiesPath))
                            existingParameters += string.Format("--cookies \"{0}\"", ms_cookiesPath);
                        else
                            MelonLoader.MelonLogger.Warning("Cookies file not found in: '" + ms_cookiesPath + "'");
                        break;
                    case Settings.CookieMode.BrowserFirefox:
                        existingParameters += "--cookies-from-browser firefox";
                        break;
                    case Settings.CookieMode.BrowserBrave:
                        existingParameters += "--cookies-from-browser brave";
                        break;
                    case Settings.CookieMode.BrowserChrome:
                        existingParameters += "--cookies-from-browser chrome";
                        break;
                    case Settings.CookieMode.BrowserChromium:
                        existingParameters += "--cookies-from-browser chromium";
                        break;
                    case Settings.CookieMode.BrowserEdge:
                        existingParameters += "--cookies-from-browser edge";
                        break;
                    case Settings.CookieMode.BrowserOpera:
                        existingParameters += "--cookies-from-browser opera";
                        break;
                    case Settings.CookieMode.BrowserSafari:
                        existingParameters += "--cookies-from-browser safari";
                        break;
                    case Settings.CookieMode.BrowserVivaldi:
                        existingParameters += "--cookies-from-browser vivaldi";
                        break;
                    case Settings.CookieMode.BrowserWhale:
                        existingParameters += "--cookies-from-browser whale";
                        break;
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Warning(e);
            }
        }
    }
}

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
            Settings.Init();
            HarmonyInstance.Patch(typeof(YoutubeDl).GetMethod("GetVideoMetaDataAsync", BindingFlags.NonPublic | BindingFlags.Static),
                new HarmonyLib.HarmonyMethod(typeof(VideoPlayerCookies).GetMethod(nameof(OnGetYoutubeVideoMetaData_Prefix), BindingFlags.NonPublic | BindingFlags.Static))
            );

            ms_cookiesPath = Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, "cookies.txt");
        }

        static void OnGetYoutubeVideoMetaData_Prefix(ref string parameter)
        {
            try
            {
                if (!Settings.Enabled)
                    return;

                switch (Settings.Mode)
                {
                    case Settings.CookieMode.File:
                        if (File.Exists(ms_cookiesPath))
                            parameter += string.Format(" --cookies \"{0}\"", ms_cookiesPath);
                        else
                            MelonLoader.MelonLogger.Warning("Cookies file not found in: '" + ms_cookiesPath + "'");
                        break;
                    case Settings.CookieMode.BrowserFirefox:
                        parameter += " --cookies-from-browser firefox";
                        break;
                    case Settings.CookieMode.BrowserBrave:
                        parameter += " --cookies-from-browser brave";
                        break;
                    case Settings.CookieMode.BrowserChrome:
                        parameter += " --cookies-from-browser chrome";
                        break;
                    case Settings.CookieMode.BrowserChromium:
                        parameter += " --cookies-from-browser chromium";
                        break;
                    case Settings.CookieMode.BrowserEdge:
                        parameter += " --cookies-from-browser edge";
                        break;
                    case Settings.CookieMode.BrowserOpera:
                        parameter += " --cookies-from-browser opera";
                        break;
                    case Settings.CookieMode.BrowserSafari:
                        parameter += " --cookies-from-browser safari";
                        break;
                    case Settings.CookieMode.BrowserVivaldi:
                        parameter += " --cookies-from-browser vivaldi";
                        break;
                    case Settings.CookieMode.BrowserWhale:
                        parameter += " --cookies-from-browser whale";
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

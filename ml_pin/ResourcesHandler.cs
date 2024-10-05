using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ml_pin
{
    static class ResourcesHandler
    {
        const string c_modName = "PlayersInstanceNotifier";
        readonly static string ms_namespace = typeof(ResourcesHandler).Namespace;

        static readonly List<string> ms_audioResources = new List<string>()
        {
            "Chime.wav",
            "DoorClose.wav"
        };

        public static void ExtractAudioResources()
        {
            string l_dirPath = MelonLoader.Utils.MelonEnvironment.UserDataDirectory;
            if(!Directory.Exists(l_dirPath))
                Directory.CreateDirectory(l_dirPath);

            l_dirPath = Path.Combine(l_dirPath, c_modName);
            if(!Directory.Exists(l_dirPath))
                Directory.CreateDirectory(l_dirPath);

            string l_filePath = Path.Combine(l_dirPath, "player_join.wav");
            if(!File.Exists(l_filePath))
                ExtractAudioFile(ms_audioResources[0], l_filePath);

            l_filePath = Path.Combine(l_dirPath, "player_leave.wav");
            if(!File.Exists(l_filePath))
                ExtractAudioFile(ms_audioResources[1], l_filePath);

            l_filePath = Path.Combine(l_dirPath, "friend_join.wav");
            if(!File.Exists(l_filePath))
                ExtractAudioFile(ms_audioResources[0], l_filePath);

            l_filePath = Path.Combine(l_dirPath, "friend_leave.wav");
            if(!File.Exists(l_filePath))
                ExtractAudioFile(ms_audioResources[1], l_filePath);
        }

        static void ExtractAudioFile(string p_name, string p_path)
        {
            Assembly l_assembly = Assembly.GetExecutingAssembly();

            try
            {
                Stream l_resourceStream = l_assembly.GetManifestResourceStream(ms_namespace + ".resources." + p_name);
                Stream l_fileStream = File.Create(p_path);
                l_resourceStream.CopyTo(l_fileStream);
                l_fileStream.Flush();
                l_fileStream.Close();
                l_resourceStream.Close();
            }
            catch(Exception)
            {
                MelonLoader.MelonLogger.Warning("Unable to write '" + p_path + "' file, problems can occur.");
            }
        }

        public static string GetEmbeddedResource(string p_name)
        {
            string l_result = "";
            Assembly l_assembly = Assembly.GetExecutingAssembly();

            try
            {
                Stream l_libraryStream = l_assembly.GetManifestResourceStream(ms_namespace + ".resources." + p_name);
                StreamReader l_streadReader = new StreamReader(l_libraryStream);
                l_result = l_streadReader.ReadToEnd();
            }
            catch(Exception) { }

            return l_result;
        }
    }
}

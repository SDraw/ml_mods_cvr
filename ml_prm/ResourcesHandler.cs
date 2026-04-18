using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ml_prm
{
    static class ResourcesHandler
    {
        const string c_modName = "PlayerRagdollMod";
        readonly static string ms_namespace = typeof(ResourcesHandler).Namespace;

        static readonly List<string> ms_audioResources = new List<string>()
        {
            "body_medium_impact_hard1.wav",
            "body_medium_impact_hard2.wav",
            "body_medium_impact_hard3.wav",
            "body_medium_impact_hard4.wav",
            "body_medium_impact_hard5.wav",
            "body_medium_impact_hard6.wav",
            "body_medium_impact_soft1.wav",
            "body_medium_impact_soft2.wav",
            "body_medium_impact_soft3.wav",
            "body_medium_impact_soft4.wav",
            "body_medium_impact_soft5.wav",
            "body_medium_impact_soft6.wav",
            "body_medium_impact_soft7.wav"
        };

        public static void ExtractResources()
        {
            string l_dirPath = MelonLoader.Utils.MelonEnvironment.UserDataDirectory;
            if(!Directory.Exists(l_dirPath))
                Directory.CreateDirectory(l_dirPath);

            l_dirPath = Path.Combine(l_dirPath, c_modName);
            if(!Directory.Exists(l_dirPath))
                Directory.CreateDirectory(l_dirPath);

            foreach(string l_name in ms_audioResources)
            {
                string l_filePath = Path.Combine(l_dirPath, l_name);
                if(!File.Exists(l_filePath))
                    ExtractAudioFile(l_name, l_filePath);
            }
        }

        static void ExtractAudioFile(string p_name, string p_path)
        {
            Assembly l_assembly = Assembly.GetExecutingAssembly();

            try
            {
                Stream l_resourceStream = l_assembly.GetManifestResourceStream(ms_namespace + ".resources.sounds." + p_name);
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
    }
}

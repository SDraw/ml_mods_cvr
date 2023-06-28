using System;
using System.IO;
using System.Reflection;

namespace ml_fpt
{
    static class Scripts
    {
        public static string GetEmbeddedScript(string p_name)
        {
            string l_result = "";
            Assembly l_assembly = Assembly.GetExecutingAssembly();
            string l_assemblyName = l_assembly.GetName().Name;

            try
            {
                Stream l_libraryStream = l_assembly.GetManifestResourceStream(l_assemblyName + ".resources." + p_name);
                StreamReader l_streadReader = new StreamReader(l_libraryStream);
                l_result = l_streadReader.ReadToEnd();
            }
            catch(Exception) { }

            return l_result;
        }
    }
}

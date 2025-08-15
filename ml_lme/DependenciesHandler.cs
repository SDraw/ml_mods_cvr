using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace ml_lme
{
    static class DependenciesHandler
    {
        readonly static string ms_namespace = typeof(DependenciesHandler).Namespace;

        static readonly List<string> ms_libraries = new List<string>()
        {
            "LeapC.dll"
        };

        public static void ExtractDependencies()
        {
            Assembly l_assembly = Assembly.GetExecutingAssembly();

            foreach(string l_library in ms_libraries)
            {
                Stream l_libraryStream = l_assembly.GetManifestResourceStream(ms_namespace + ".resources." + l_library);

                if(!File.Exists(l_library))
                {
                    try
                    {
                        Stream l_fileStream = File.Create(l_library);
                        l_libraryStream.CopyTo(l_fileStream);
                        l_fileStream.Flush();
                        l_fileStream.Close();
                    }
                    catch(Exception)
                    {
                        MelonLoader.MelonLogger.Error("Unable to extract embedded " + l_library + " library");
                    }
                }
                else
                {
                    try
                    {
                        FileStream l_fileStream = File.Open(l_library, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                        SHA256 l_hasher = SHA256.Create();
                        byte[] l_libraryHash = l_hasher.ComputeHash(l_libraryStream);
                        byte[] l_fileHash = l_hasher.ComputeHash(l_fileStream);

                        for(int i = 0; i < l_libraryHash.Length; i++)
                        {
                            if(l_libraryHash[i] != l_fileHash[i])
                            {
                                l_fileStream.SetLength(l_libraryStream.Length);
                                l_fileStream.Position = 0;
                                l_libraryStream.Position = 0;
                                l_libraryStream.CopyTo(l_fileStream);
                                l_fileStream.Flush();

                                MelonLoader.MelonLogger.Msg("Updated " + l_library + " library from embedded one");

                                break;
                            }
                        }

                        l_fileStream.Close();
                    }
                    catch(Exception)
                    {
                        MelonLoader.MelonLogger.Error("Unable to compare/update " + l_library + " library, delete it from game folder manually and restart.");
                    }
                }

                l_libraryStream.Close();
            }
        }
    }
}

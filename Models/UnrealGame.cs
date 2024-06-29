using SharpCompress;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWSManager.Models
{
    // This class is primarily meant to deal with UE4 games (probably UE5 too). UE3 and below are not supported.
    public class UE4Game : Game
    {
        public string UnrealName = "";
        public UE4Game(string name, string path, dynamic? extraData = null) : base(name, path, extraData) {
            UnrealName = extraData?.UnrealName ?? name;

            ModFileDirs.Add($"{UnrealName}/Content/Paks/~mods/*.pak");
            ModFileDirs.Add($"{UnrealName}/Content/Paks/LogicMods/*.pak");
            ModDirs.Add($"{UnrealName}/Binaries/Win64/Mods");
            ModDirs.Add($"{UnrealName}/Content/Paks/LogicMods");
        }

        protected override void ProcessMod(Mod mod, string modDir)
        {
            modDir = modDir.Replace($"{Name}/", "");
            if (modDir.Contains("*"))
            {
                modDir = Path.GetDirectoryName(modDir);
            }

            switch(modDir)
            {
                case "Content/Paks/~mods": 
                {
                    ProcessPakMod(mod);
                    break;
                }
                case "Content/Paks/LogicMods":
                {
                    ProcessPakMod(mod);
                    break;
                }
                case "Binaries/Win64/Mods":
                {
                    ProcessPakMod(mod);
                    break;
                }
                default:
                {
                    Trace.TraceError("Invalid mod directory");
                    break;
                }
            };
        }

        protected void ProcessPakMod(Mod mod)
        {

        }

        public override void ExtractAndInstallMod(FileData fileData, Stream stream, string installPath)
        {
            stream.Position = 0;

            using var archiveReader = ReaderFactory.Open(stream);
            archiveReader.WriteAllToDirectory(Path.Combine(GamePath, installPath), new SharpCompress.Common.ExtractionOptions
            {
                ExtractFullPath = true,
                Overwrite = true
            });
        }

        protected override string GetModInstallPath(FileData fileData, InstallMethod method)
        {
            if (method == InstallMethod.Install)
            {
                
            } 
            else
            {
                Trace.WriteLine(fileData.Stream.CanRead);
                var archiveReader = ReaderFactory.Open(fileData.Stream);
            
                //TODO: attempt to detect some common ways to install. Reject otherwise.

                while (archiveReader.MoveToNextEntry())
                {
                    var entry = archiveReader.Entry;
                    if (!entry.IsDirectory)
                    {
                        var fileName = Path.GetFileName(entry.Key);
                        var fileExt = Path.GetExtension(fileName);

                        // UE4SS mods always contain a main.lua or main.dll file
                        if (fileName == "main.lua" || fileName == "main.dll")
                        {
                            return $"{UnrealName}/Binaries/Win64/Mods";
                        } else if (fileExt == ".utoc")
                        {
                            // While this is not the smartest way, I believe it should more or less work.
                            // I tried using CUE4Parse, but the docs are quite lacking,
                            // It's possible also that some ucas/utoc isn't supported by it.
                            var stream = archiveReader.OpenEntryStream();
                            var reader = new StreamReader(stream);
                            var st = reader.ReadToEnd();

                            if (st.Contains("ModActor.uasset"))
                            {
                                return $"{UnrealName}/Content/Paks/LogicMods";
                            }
                        }
                    }
                }
            }

            return "";
        }
    }
}

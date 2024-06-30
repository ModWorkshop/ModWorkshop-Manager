using Avalonia.Controls;
using MWSManager.Services;
using Serilog;
using SharpCompress.Archives;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWSManager.Models;

// Collectoin of mod files
public class ModInstall
{
    static List<string> KnownArchiveTypes = ["zip", "7z", "rar", "gz", "tar", "xz", "gzip", "bzip2"];

    // The original file name
    public string OrigName { get; set; }

    // The original type (extension) of the file
    public string OrigType { get; set; }

    // The path of the mod install, generally in some temporary path
    public string TempPath { get; init; }

    public List<string> Files = [];
    public List<string> Folders = [];

    public ModInstall(string name, string type, Stream stream)
    {
        OrigName = name;
        OrigType = type;

        var mwsTemp = Path.Combine(Path.GetTempPath(), "mws-manager");
        TempPath = Path.Combine(mwsTemp, Path.GetFileNameWithoutExtension(name) + "_" +  Path.GetFileNameWithoutExtension(Path.GetTempFileName()));

        if (!Directory.Exists(TempPath))
        {
            Directory.CreateDirectory(TempPath);
        }

        // Generally assume that if it's an archive file it needs to be extracted.
        // It's possible we'll need to make special cases for some games. At the moment, I don't know any game that needs such things.
        if (KnownArchiveTypes.Contains(type))
        {
            // Special case for 7z
            stream.Position = 0;

            var factory = ArchiveFactory.Open(stream);
            foreach(var entry in factory.Entries)
            {
                if (entry.Key != null)
                {
                    if (entry.IsDirectory)
                    {
                        Folders.Add(entry.Key);
                    }
                    else
                    {
                        Files.Add(entry.Key);
                    }
                }
            }

           factory.WriteToDirectory(TempPath,
                new SharpCompress.Common.ExtractionOptions
                {
                    ExtractFullPath = true,
                    Overwrite = true
                }
            );
        } else
        {
            using var tempFile = File.OpenWrite(Path.Combine(TempPath, name));
            stream.CopyTo(tempFile);
            Files.Add(name);
        }
    }

    // Moves all files into some directory
    public void MoveAll(string dir)
    {
        Log.Information("Moving all files to {0}", dir);
        foreach(var folder in Folders)
        {
            Directory.CreateDirectory(Path.Combine(dir, folder));
            Log.Information("Move {0} to {1}", GetRealPath(folder), Path.Combine(dir, folder));

            Utils.CopyDirectory(GetRealPath(folder), Path.Combine(dir, folder));
        }

        foreach(var file in Files)
        {
            if (!file.Contains("/"))
            {
                //TODO: handle possibly warning the user about overwrite?
                File.Copy(GetRealPath(file), Path.Combine(dir, file), true);
            }
        }

        Log.Information("Cleaning folder from temp path");
        Directory.Delete(TempPath, true);
    }

    public string GetRealPath(string path)
    {
        return Path.Combine(TempPath, path);
    }
}

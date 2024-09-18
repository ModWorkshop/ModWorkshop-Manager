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

public class ModInstall
{
    static List<string> KnownArchiveTypes = ["zip", "7z", "rar", "gz", "tar", "xz", "gzip", "bzip2"];

    // The path of the mod install, generally in some temporary path
    public string TempPath { get; init; }

    public List<string> Files = [];
    public List<string> Folders = [];

    public ModUpdate Update { get; }

    public bool SingleFile => Files.Count == 1 && Folders.Count == 0;

    public ModInstall(ModUpdate update, string fileName, Stream stream)
    {
        Update = update;

        var mwsTemp = Path.Combine(Path.GetTempPath(), "mws-manager");
        var type = Path.GetExtension(fileName).Replace(".", "");

        TempPath = Path.Combine(mwsTemp, Path.GetFileNameWithoutExtension(fileName) + "_" +  Path.GetFileNameWithoutExtension(Path.GetTempFileName()));

        if (!Directory.Exists(TempPath))
        {
            Directory.CreateDirectory(TempPath);
        }

        // Generally assume that if it's an archive file it needs to be extracted.
        // It's possible we'll need to make special cases for some games. At the moment, I don't know any game that needs such things.
        if (KnownArchiveTypes.Contains(type))
        {
            stream.Position = 0;

            var factory = ArchiveFactory.Open(stream);
            foreach(var entry in factory.Entries)
            {
                if (entry.Key != null)
                {
                    if (entry.IsDirectory)
                        Folders.Add(entry.Key);
                    else
                        Files.Add(entry.Key);
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
            using var tempFile = File.OpenWrite(Path.Combine(TempPath, fileName));
            stream.CopyTo(tempFile);
            Files.Add(fileName);
        }
    }

    // Moves file/folder into some directory
    public void Move(string dir)
    {
        try
        {
            if (SingleFile)
            {
                var file = GetRealPath(Files[0]);
                Update.Mod.ModPath = Path.Combine(dir, Path.GetFileName(file));
                Log.Information("Move File {0} -> {1}", file, Update.Mod.ModPath);
                File.Copy(file, Update.Mod.ModPath);
                Log.Information("Move Success");
            }
            else
            {
                var folder = GetRealPath(Folders[0]);
                Update.Mod.ModPath = Path.Combine(dir, Path.GetFileName(Path.GetDirectoryName(folder)));
                Log.Information("Move {0} -> {1}", folder, Update.Mod.ModPath);
                Utils.CopyDirectory(folder, Update.Mod.ModPath);
                Log.Information("Move Success");
            }

            Log.Information("Cleaning folder from temp path");
            Directory.Delete(TempPath, true);
        }
        catch (Exception e)
        {
            Log.Error("Couldn't move: {0}", e);
        }

    }

    public string GetRealPath(string path)
    {
        return Path.Combine(TempPath, path);
    }
}

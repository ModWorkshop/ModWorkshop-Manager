using Avalonia.Controls;
using MWSManager.Services;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWSManager.Models;

public enum InstallMethod {
    Install, // Install the mod as-is. Example: pak files
    ExtractAndInstall, // Extract the mod and install it. Example: zipped PD2 mods.
    Other, // Do something else, requires implementing some kind of installation.
    Unknown // If you determine that the format of the mod is just unknown or we aren't sure where to place it.
}

public class FileData
{
    static List<string> KnownArchiveTypes = ["zip", "7z", "rar", "gz", "tar", "xz", "gzip", "bzip2"];

    // The file name
    public required string Name { get; set; }
    
    // The type (extension) of the file
    public required string Type { get; set; }

    // The stream of the file as it gets downloaded
    public Stream? Stream { get; set; }

    public bool IsArchive => KnownArchiveTypes.Contains(Type);
}

public class Game
{
    #region Properties
    // The name of the game
    public string Name { get; init; } = "";

    // The ID on MWS
    public uint? MWSId { get; set; }

    // The path to the game
    public string GamePath { get; init; }

    // If the game contains some extra info required for it (Like unreal's name)
    public dynamic? ExtraData { get; init; }

    public ObservableCollection<Mod> Mods { get; } = [];

    public string? Thumbnail { get; set; }

    // Directories to load mods from (Mods that are directories)
    public List<string> ModDirs = [];

    // Directories to load mod files from (Mods that are files like .pak)
    public List<string> ModFileDirs = [];

    public List<string> IgnoreModNames = [];


    protected bool HasLoadedMods = false;

    #endregion

    #region methods
    public Game(string name, string path, dynamic? extraData = null) {
        Name = name;
        GamePath = path;
        ExtraData = extraData;
    }

    public void LookForMods(bool force = false)
    {
        if (HasLoadedMods && !force)
        {
            return;
        }

        HasLoadedMods = true;

        foreach (var path in ModDirs)
        {
            var dir = $"{GamePath}/{path}";
            if (Directory.Exists(dir))
            {
                foreach (var folder in Directory.GetDirectories(dir))
                {
                    LoadMod(folder, path);
                }
            }
        }

        foreach (var path in ModFileDirs)
        {
            var dirPattern = $"{GamePath}/{path}";
            var modsDir = Path.GetDirectoryName(dirPattern);
            if (Directory.Exists(modsDir)) {
                foreach (var file in Directory.GetFiles(modsDir, Path.GetFileName(dirPattern)))
                {
                    LoadMod(file, path);
                }
            }
        }
    }

    public void LoadMod(string path, string modDir)
    {
        var name = Path.GetFileName(path);
        if (IgnoreModNames.Contains(name))
        {
            return;
        }

        //TODO: in some cases name can be taken from places like mod.txt, however prefer to read the MWSManager.json file.
        var mod = new Mod { Name = name, ModPath = path };
        ProcessMod(mod, modDir);
        Mods.Add(mod);
    }

    // Processes the mod, tries to load data like mod.txt and such to figure name, version, etc.
    protected virtual void ProcessMod(Mod mod, string modDir)
    {
        
    }

    public void TryInstallMod(FileData fileData)
    {
        if (fileData.Type == "pdmod")
        {
            return; //unsupported!
        }

        if (fileData.Stream == null)
        {
            throw new Exception("The mod contains no stream to install from!");
        }

        var method = GetModInstallInstallMethod(fileData);

        if (method == InstallMethod.Install)
        {
            InstallMod(fileData, fileData.Stream, GetModInstallPath(fileData, method));
        } 
        else if (method == InstallMethod.ExtractAndInstall)
        {
            ExtractAndInstallMod(fileData, fileData.Stream, GetModInstallPath(fileData, method));
        }
        else
        {
            // Not implemented
        }
        
    }

    public virtual void ExtractAndInstallMod(FileData fileData, Stream strea, string installPath)
    {

    }

    public virtual void InstallMod(FileData fileData, Stream strea, string installPath)
    {

    }

    // This method runs BEFORE trying to do anything with the file.
    // If your game needs to do some setup with the file or it reads archive files directly,
    // you should override this function in your game class.
    public virtual InstallMethod GetModInstallInstallMethod(FileData fileData)
    {
        if (fileData.IsArchive)
        {
            return InstallMethod.ExtractAndInstall;
        } else
        {
            return InstallMethod.Other;
        }
    }

    // Returns where a mod should be installed. If your game contains multiple you must handle this yourself
    protected virtual string GetModInstallPath(FileData fileData, InstallMethod method)
    {
        if (ModFileDirs.Count == 1)
        {
            return ModFileDirs[0];
        } else
        {
            throw new Exception("Cannot determine which folder to install the mod to. Please handle this manually (GetModInstallPath)");
        }
    }
}

#endregion methods

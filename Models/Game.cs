using Avalonia.Controls;
using GameFinder.Common;
using MWSManager.Services;
using Newtonsoft.Json;
using Serilog;
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

public class Game
{
    #region Properties
    // The name of the game
    public string Name { get; init; }

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

    public Dictionary<string, string> SpecialPaths = [];


    protected bool HasLoadedMods = false;

    public Game(string name, string gamePath, dynamic? extraData = null)
    {
        ExtraData = extraData;
        GamePath = gamePath;
        Name = name;
    }

    #endregion

    #region methods

    public void LookForMods(bool force = false)
    {
        if (HasLoadedMods && !force)
        {
            return;
        }

        if (HasLoadedMods)
        {
            Mods.Clear();
        }

        HasLoadedMods = true;

        foreach (var path in ModDirs)
        {
            var dir = $"{GamePath}/{path}";
            if (Directory.Exists(dir))
            {
                foreach (var folder in Directory.GetDirectories(dir))
                {
                    LoadMod(folder);
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
                    LoadMod(file);
                }
            }
        }
    }

    public void LoadMod(string path)
    {
        var name = Path.GetFileName(path);
        if (IgnoreModNames.Contains(name))
        {
            return;
        }

        var mod = new Mod(path);
        mod.Game = this;
        ProcessMod(mod);
        Mods.Add(mod);
    }

    // Processes the mod, tries to load data like mod.txt and such to figure name, version, etc.
    protected virtual void ProcessMod(Mod mod) { }

    public void TryInstallMod(ModInstall install)
    {
        var installPath = GetModInstallPath(install);
        if (installPath != null) {
            Log.Information("Installing mod in {0}", installPath);

            install.Move(Path.Combine(GamePath, installPath));

            var mod = install.Update.Mod;
            mod.LoadSchema(!install.Update.FreshInstall);
            ProcessMod(install.Update.Mod);

            if (install.Update.FreshInstall)
                Mods.Add(mod);
        } else {
            Log.Error("Could not determine a directory to install the mod in!");
        }
    }

    protected string? GetModSchemaInstallPath(ModInstall install)
    {
        foreach (var filePath in install.Files)
        {
            var fileName = Path.GetFileName(filePath);
            // Ensure it's only root level files
            if (fileName == "mws-manager.json" && filePath.Replace("\\", "/").Split("/").Length == 2)
            {
                var schema = File.ReadAllText(install.GetRealPath(filePath));
                var modSchema = JsonConvert.DeserializeObject<ModSchema>(schema);

                if (modSchema != null && modSchema.installDir != null)
                {
                    var dir = modSchema.installDir;
                    foreach(var special in SpecialPaths)
                    {
                        dir = dir.Replace($"%{special.Key}%", special.Value);
                    }
                    return dir;
                }

                break;
            }
        }

        return null;
    }

    // Returns where a mod should be installed. If your game contains multiple places you must handle this yourself
    protected virtual string? GetModInstallPath(ModInstall install)
    {
        var installPath = GetModSchemaInstallPath(install);
        if (installPath != null)
            return installPath;

        if (ModFileDirs.Count == 1)
        {
            return ModFileDirs[0];
        } else
        {
            return null;
        }
    }

    public void AddSpecialPath(string name, string path)
    {
        SpecialPaths.Add(name, path);
    }
}

#endregion methods

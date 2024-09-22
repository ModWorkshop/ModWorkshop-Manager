using MWSManager.Structures;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

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

        new Mod(this, path).Register();
    }

    // Processes the mod, tries to load data like mod.txt and such to figure name, version, etc.
    protected virtual void ProcessMod(Mod mod) { }

    public void AddMod(Mod mod)
    {
        ProcessMod(mod);
        Mods.Add(mod);
    }

    public void RemoveMod(Mod mod)
    {
        Mods.Remove(mod);
    }

    /// <summary>
    /// Parses a path that may contain placeholder paths (%UGC% -> CrimeBoss/Mods for example)
    /// </summary>
    public string ParsePath(string path)
    {
        foreach (var special in SpecialPaths)
        {
            path = path.Replace($"%{special.Key}%", special.Value);
        }
        return path;
    }

    public List<Mod> FindModsInTree(PathNode node)
    {
        List<Mod> Mods = [];

        if (node.Count == 0)
            return Mods;

        foreach (var childNode in node.ChildNodes)
        {
            CheckPossibleModInNode(childNode, Mods);
        }

        foreach (var childNode in node.ChildNodes)
        {
            Mods.AddRange(FindModsInTree(childNode));
        }

        return Mods;
    }

    public virtual void CheckPossibleModInNode(PathNode node, List<Mod> Mods)
    {
        
    }

    public void AddSpecialPath(string name, string path)
    {
        SpecialPaths.Add(name, path);
    }
}

#endregion methods

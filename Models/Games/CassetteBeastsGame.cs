using Haze.Pck;
using MWSManager.Structures;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MWSManager.Models.Games;

public class CassetteBeastsGame : Game
{
    public CassetteBeastsGame(string name, string path, dynamic? extraData = null) : base(name, path)
    {
        ScanOnlyGamePath = false;
        ModFileDirs = [ // Mods can be loaded from appdata too
            GamePath + "/mods/*.pck", 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CassetteBeasts/mods/*.pck")
        ];
    }

    protected override void ProcessMod(Mod mod)
    {
        // Since CB doesn't allow for folder-based mods, we need to read the PSK files directly.
        // TODO: possibly fork the PSK reader to ignore duplicate entries,
        // not sure how common it is but I saw at least one mod causing issues.
        // TODO: allow for reading mws-manager.json from the same path as the psk file, possible having it named like mod-name.mws-manager
        try
        {
            var pck = PckFile.Open(mod.ModPath);

            foreach (var entry in pck.Entries)
            {
                if (entry.Path.StartsWith("res://mods/"))
                {
                    var splt = entry.Path.Replace("res://mods/", "").Split("/");
                    if (splt.Length > 1 && splt[1] == "mws-manager.tres")
                    {
                        mod.LoadMetadataFromString(new StreamReader(entry.Open()).ReadToEnd());
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Error("Could not read PSK file: {0}", e);
        }
    }

    public override bool CheckPossibleModInNode(PathNode node, List<Mod> Mods)
    {
        // Detects psk files and installs them in the mods folder
        if (node.IsFile && Path.GetExtension(node.Name) == ".pck")
        {
            Mods.Add(new Mod(this, node.FullPath, "mods"));
            return true;
        }

        return false;
    }
}

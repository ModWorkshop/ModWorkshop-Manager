using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWSManager.Models.Games.Diesel;

public class RAIDWW2Game : PD2Game
{
    public RAIDWW2Game(string name, string path, dynamic? extraData = null) : base(name, path)
    {
        ModDirs = ["mods", "Maps"];
        SpecialPaths.Clear();
        ModOverridesDir = "mods";

        SpecialPaths.Add("MODS", "mods");
        SpecialPaths.Add("OVERRIDES", "mods");
        SpecialPaths.Add("MAPS", "Maps");
    }
}

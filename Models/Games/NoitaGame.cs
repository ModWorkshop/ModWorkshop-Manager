using MWSManager.Models.Games.Diesel;
using MWSManager.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MWSManager.Models.Games;

public class NoitaGame : Game
{
    public NoitaGame(string name, string path, dynamic? extraData = null) : base(name, path)
    {
        ModDirs = ["mods"];
        SpecialPaths.Add("MODS", "mods");
    }

    protected override void ProcessMod(Mod mod)
    {
        LoadModXML(mod);
    }


    private void LoadModXML(Mod mod)
    {
        var modFile = $"{mod.ModPath}/mod.xml";

        if (!File.Exists(modFile))
        {
            return;
        }

        XmlReaderSettings settings = new XmlReaderSettings();
        // Required for the mess known as custom xml lol
        settings.ConformanceLevel = ConformanceLevel.Fragment;

        using var stream = new FileStream(modFile, FileMode.Open);
        using var reader = XmlReader.Create(stream, settings);

        while (reader.Read())
        {
            bool found = false;
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    {
                        if (reader.Name == "Mod")
                        {
                            var name = reader.GetAttribute("name");
                            var desc = reader.GetAttribute("description");
                            if (name != null)
                            {
                                mod.Name = name;
                            }
                            if (desc != null)
                            {
                                mod.Desc = desc;
                            }
                        }
                        break;
                    }
            }
            if (found)
            {
                break;
            }
        }
    }

    public override bool CheckPossibleModInNode(PathNode node, List<Mod> Mods)
    {
        if (node.IsFile && node.Name == "mod.xml")
        {
            Mods.Add(new Mod(this, node.Parent!.FullPath, "mods"));
            return true;
        }

        return false;
    }
}

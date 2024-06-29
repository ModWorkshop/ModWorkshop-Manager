using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWSManager.Models
{
    public class Mod
    {
        // The ID of the mod like the MWS mod ID.
        public string Id { get; set; }

        // The name of the mod
        public string Name { get; set; }

        // The owner/author of the mod
        public string Owner { get; set; }

        // The version of the mod
        public string Version { get; set; }

        // The path to the mod
        public string ModPath { get; set; }

        // URL or path to the image of the mod
        public string Thumbnail { get; set; } = "../Assets/DefaultModThumb.png";
    }
}

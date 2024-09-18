using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWSManager.Models
{
    public class ModSchemaUpdate
    {
        public required string provider;
        public required string id;
    }

    public class ModSchema
    {
        [Description("The name of the mod")]
        public string name = "Missing Name";
        public string desc = "";
        public string[] authors = [];
        public string version = "";

        public string? thumbnail;

        public string? installDir;

        public ModSchemaUpdate[] updates = [];
        //TODO: dependencies?
    }
}

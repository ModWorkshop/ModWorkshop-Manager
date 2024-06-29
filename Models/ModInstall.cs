using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWSManager.Models
{
    // Collectoin of mod files
    class ModInstall
    {
        // The path of the mod install, generally in some temporary path
        public string TempPath { get; init; }

        public void GetDirectories()
        {
            //return Directory.GetDirectories(TempPath);
        }
    }
}

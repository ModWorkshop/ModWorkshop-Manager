using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MWSManager.Services
{
    internal class Utils
    {
        static readonly HttpClient client = new();

        public static HttpClient GetHTTPClient()
        {
            return client;
        }

        public static void CopyDirectory(string directory, string destinationDir)
        {
            foreach (string dir in Directory.GetDirectories(directory, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dir.Replace(directory, destinationDir));
            }

            foreach (string newPath in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(directory, destinationDir), true);
            }
        }
    }
}

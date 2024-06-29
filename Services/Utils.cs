using System;
using System.Collections.Generic;
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
    }
}

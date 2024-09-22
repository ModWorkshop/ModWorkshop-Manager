using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWSManager.Services;

public class PathUtils
{
    /// <summary>
    /// Returns a "normalized" path by making sure it's using forward slashes and has no trailing slashes
    /// </summary>
    public static string NormalizePath(string path)
    {
        var str = path.Length > 0 ? path.Replace("\\", "/") : "";
        if (str.EndsWith("/"))
        {
            str = str.Substring(0, str.Length - 1);
        }

        return str;
    }
}

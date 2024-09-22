using MWSManager.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace MWSManager.Structures;

/// <summary>
/// A simple TreeNode. Used mainly for easier file traversal
/// </summary>
public class PathNode(string name, bool isFile = false, PathNode? parent = null)
{
    public string Name = PathUtils.NormalizePath(name);
    public bool IsFile = isFile;

    /// <summary>
    /// Returns the full path of a node by going up the tree
    /// </summary>
    public string FullPath
    {
        get {
            var s = "";
            PathNode? curr = this;

            while (curr != null)
            {
                s = Path.Combine(curr.Name, s);
                curr = curr.Parent;
            }

            return s;
        }
    }

    public PathNode? Parent = parent;
    public Dictionary<string, PathNode> Children { get; private set; } = [];

    public int Count => Children.Count;

    public ICollection<PathNode> ChildNodes => Children.Values;

    public int Height()
    {
        int h = 0;
        foreach(var node in Children)
        {
            h = Math.Max(h, 1 + node.Value.Height());
        }

        return h;
    }

    public PathNode? GetChild(string name) => Children.GetValueOrDefault(name);

    public bool Contains(string name) => Children.ContainsKey(name);

    public PathNode? Add(string path, bool isFile = false)
    {
        var splt = PathUtils.NormalizePath(path).Replace(FullPath + "/", "").Split("/");
        var node = this;

        for (int i = 0; i < splt.Length; i++)
        {
            var next = node.GetChild(splt[i]);
            if (next == null)
            {
                next = new PathNode(splt[i], i == splt.Length - 1 && isFile, node);
                node.Children.Add(splt[i], next);

                Log.Information("Add {0}->{1}", node.FullPath, splt[i]);
            }

            node = next;
        }

        return node;
    }

    public bool Remove(PathNode value)
    {
        return Children.Remove(value.Name);
    }
}

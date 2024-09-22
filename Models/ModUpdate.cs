using CommunityToolkit.Mvvm.ComponentModel;
using MWSManager.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWSManager.Models;

public partial class ModUpdate : ReactiveObject
{
    public Mod? Mod { get; }

    public Game Game { get; }

    public string Provider { get; }
    public string Id { get; }

    public string Name { get; set; }

    [Reactive]
    private string version;

    [Reactive]
    private string? nextVersion;

    [Reactive]
    private double downloadProgress;

    public bool FreshInstall { get; set; } = true;

    /// <summary>
    /// Creates a mod update for a mod
    /// </summary>
    public ModUpdate(Mod mod, string provider, string id, string version) : this(mod.Game, mod.Name, provider, id, version)
    {
        Mod = mod;
        FreshInstall = false;
    }

    /// <summary>
    /// Creates a mod update for a fresh install
    /// </summary>
    public ModUpdate(Game game, string name, string provider, string id, string version)
    {
        Name = name;
        Game = game;
        Provider = provider;
        Id = id;
        Version = version;
    }

    public void RaiseHasUpdate(string version)
    {
        Log.Information("Received next version! {0}", version);
        NextVersion = version;
    }

    public void DownloadAndInstallUpdate()
    {
        UpdatesService updates = UpdatesService.Instance;
        updates.DownloadAndInstall(this);
    }
}


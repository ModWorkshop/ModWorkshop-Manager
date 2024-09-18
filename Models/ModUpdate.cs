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

public partial class ModUpdate(Mod mod, string provider, string id, string version) : ReactiveObject
{
    public Mod Mod { get; } = mod;

    public string Provider { get; } = provider;
    public string Id { get; } = id;

    [Reactive]
    private string version = version;

    [Reactive]
    private string? nextVersion;

    [Reactive]
    private double downloadProgress;

    public bool FreshInstall { get; set; }

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


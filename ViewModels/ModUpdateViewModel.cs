using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MWSManager.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MWSManager.ViewModels;

public partial class ModUpdateViewModel : ViewModelBase
{
    [Reactive]
    private ModUpdate update;

    [ObservableAsProperty]
    public double downloadPercent;

    [Reactive]
    private bool canUpdate = true;

    public ReactiveCommand<Unit, Unit> DownloadUpdate { get; }

    public ModUpdateViewModel(ModUpdate update)
    {
        Update = update;

        var canExecute = this.WhenAnyValue(x => x.CanUpdate);

        DownloadUpdate = ReactiveCommand.Create(() =>
        {
            CanUpdate = false;
            Update.DownloadAndInstallUpdate();
        }, canExecute);

        downloadPercentHelper = this.WhenAnyValue(x => x.Update.DownloadProgress)
                .Select(x => x * 100)
                .ToProperty(this, x => x.DownloadPercent);

        if (update.FreshInstall)
        {
            CanUpdate = false;
        }
    }
}

using DynamicData;
using DynamicData.Binding;
using MWSManager.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

namespace MWSManager.ViewModels;

public partial class DownloadsPageViewModel : PageViewModel
{
    public ObservableCollection<ModUpdateViewModel> Updates { get; } = [];

    public MainWindowViewModel? Window { get; set; }

    [ObservableAsProperty]
    public bool hasUpdates = false;

    private bool FirstOpen = false;

    public DownloadsPageViewModel()
    {
        Thumbnail = "avares://MWSManager/Assets/Download.png";

        hasUpdatesHelper = Updates.ToObservableChangeSet().Select(x => x.Count > 0).ToProperty(this, x => x.HasUpdates);

        UpdatesService updatesService = UpdatesService.Instance;



        updatesService.Updates.ToObservableChangeSet()
            .AutoRefresh(x => x.FreshInstall)
            .AutoRefresh(x => x.NextVersion)
            .Subscribe(RegUpdates => {
                foreach (var update in updatesService.Updates)
                {
                    if (!update.FreshInstall && update.NextVersion == null)
                        continue;

                    bool exists = false;
                    foreach (var muvm in Updates)
                    {
                        if (muvm.Update == update)
                            exists = true;
                    }

                    if (!exists)
                        Updates.Add(new ModUpdateViewModel(update));
                }
        });
    }

    public DownloadViewModel AddDownload(string modName, string name)
    {
        var down = new DownloadViewModel()
        {
            Name = name,
            ModName = modName
        };
        //Downloads.Add(down);

        if (Window != null)
        {
            Window.CurrentOtherPage = Window.Downloads;
        }

        return down;
    }
}
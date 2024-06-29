using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MWSManager.ViewModels;

public partial class DownloadsPageViewModel : PageViewModel
{
    public ObservableCollection<DownloadViewModel> Downloads { get; } = [];

    public MainWindowViewModel? Window { get; set; }

    public bool HasDownloads => Downloads.Count > 0;

    public DownloadsPageViewModel()
    {
        Thumbnail = "avares://MWSManager/Assets/Download.png";
    }

    public DownloadViewModel AddDownload(string modName, string name)
    {
        var down = new DownloadViewModel()
        {
            Name = name,
            ModName = modName
        };
        Downloads.Add(down);

        if (Window != null)
        {
            Window.CurrentPage = Window.Downloads;
        }

        return down;
    }
}
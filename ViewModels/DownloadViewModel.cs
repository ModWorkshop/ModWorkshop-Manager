using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace MWSManager.ViewModels;

public partial class DownloadViewModel : ViewModelBase
{
	[ObservableProperty]
	private string name = "Unknown download";

    [ObservableProperty]
    private string modName = "Unknown mod";

    [ObservableProperty]
    private int progress;

    [ObservableProperty]
    private bool showProgressPercent = true;

    [ObservableProperty]
    private string progressText = "Downloading";
}
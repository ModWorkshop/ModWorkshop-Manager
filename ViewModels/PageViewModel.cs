using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;

namespace MWSManager.ViewModels;

public partial class PageViewModel : ViewModelBase
{
	[ObservableProperty]
	protected string thumbnail = "";

}
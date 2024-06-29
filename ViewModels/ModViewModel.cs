using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using MWSManager.Models;
using ReactiveUI;

namespace MWSManager.ViewModels
{
	public partial class ModViewModel : ViewModelBase
    {
		[ObservableProperty]
        private Mod mod;
	}
}
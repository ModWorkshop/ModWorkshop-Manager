using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using MWSManager.Models;
using ReactiveUI;

namespace MWSManager.ViewModels
{
	public partial class ModInfoViewModel : ViewModelBase
	{
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasMod))]
        private Mod? mod;

        public bool HasMod => Mod != null;
    }
}
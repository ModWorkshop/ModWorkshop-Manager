using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using MWSManager.Models;
using MWSManager.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Serilog;

namespace MWSManager.ViewModels
{
	public partial class ModInfoViewModel : ViewModelBase
	{
        [Reactive]
        private Mod? mod;

        [ObservableAsProperty]
        public bool hasMod = false;

        [ObservableAsProperty]
        public string? authorsCommaSep;

        [ObservableAsProperty]
        public string thumbnail;

        public ModInfoViewModel()
        {
            hasModHelper = this.WhenAnyValue(x => x.Mod)
                .Select(x => x != null)
                .ToProperty(this, x => x.HasMod);

            authorsCommaSepHelper = this.WhenAnyValue(x => x.Mod)
                .Select(x => x?.Authors.Count > 0 ? String.Join(", ", x.Authors) : null)
                .ToProperty(this, x => x.AuthorsCommaSep);

            thumbnailHelper = this.WhenAnyValue(x => x.Mod)
                .Select(x => x?.Thumbnail ?? "../Assets/DefaultModThumb.png")
                .ToProperty(this, x => x.Thumbnail);
        }
    }
}
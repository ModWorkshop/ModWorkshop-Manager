using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
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
        private bool hasMod = false;

        [Reactive]
        private bool hasUpdates = false;

        [Reactive]
        private string? pageUrl;

        [ObservableAsProperty]
        private string? authorsCommaSep;

        [ObservableAsProperty]
        private string thumbnail;

        public ObservableCollection<ModUpdateViewModel> Updates { get; } = [];

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

            this.WhenAnyValue(x => x.Mod).WhereNotNull().Subscribe(mod =>
            {
                Updates.Clear();
                foreach (var update in mod.Updates)
                {
                    Updates.Add(new ModUpdateViewModel(update));
                }

                HasUpdates = Updates.Count > 0;
                PageUrl = mod.Id != null ? $"https://modworkshop.net/mod/{mod.Id}" : null;
            });
        }

        [ReactiveCommand]
        private void BrowseToModPath()
        {
            try
            {
                if (Mod.IsFile)
                {
                    Process.Start("explorer.exe", Path.GetDirectoryName(Mod.ModPath));
                }
                else
                {
                    Process.Start("explorer.exe", Path.GetFullPath(Mod.ModPath));
                }
            }
            catch (Exception e)
            {
                Log.Information("{0}", e);
            }
        }

        [ReactiveCommand]
        private void OpenPageUrl()
        {
            if (PageUrl != null)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(PageUrl)
                    { 
                        UseShellExecute = true
                    });
                }
                catch (Exception e)
                {
                    Log.Error("Couldn't open page URL: {0}. {1}", PageUrl, e);
                }
            }
        }
    }
}
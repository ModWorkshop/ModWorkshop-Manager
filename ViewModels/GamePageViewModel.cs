using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameFinder.Common;
using MWSManager.Models;
using MWSManager.Services;
using ReactiveUI;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;

namespace MWSManager.ViewModels;


public partial class GamePageViewModel : PageViewModel
{
    public static string[] validArchiveExt = { "zip", "7z", "rar", "xz", "gz", "tar" };

    public Game Game { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMods))]
    private List<ModViewModel> mods = [];

    [ObservableProperty]
    private ModViewModel? selectedMod;

    [ObservableProperty]
    private ModInfoViewModel modInfo;

    public bool HasMods => Mods.Count > 0;

    public GamePageViewModel(Game game) {
        Game = game;
        Thumbnail = game.Thumbnail;
        ModInfo = new ModInfoViewModel();
    }

    public void LoadMods()
    {
        Game.Mods.Clear();
        Game.LookForMods();

        foreach (var mod in Game.Mods)
        {
            Mods.Add(new ModViewModel
            {
                Mod  = mod,
            });
        }
    }

    partial void OnSelectedModChanged(ModViewModel? value)
    {
        ModInfo.Mod = value?.Mod ?? null;
    }

    public void TryInstallMod(ModInstall install)
    {
        Game.TryInstallMod(install);
    }
}

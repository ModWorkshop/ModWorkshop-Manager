using CommunityToolkit.Mvvm.ComponentModel;
using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.Steam;
using MWSManager.Models;
using MWSManager.Models.Diesel;
using NexusMods.Paths;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Platform;
using GameFinder.Common;
using GameFinder.StoreHandlers.Steam.Models;
using GameFinder.StoreHandlers.Steam.Models.ValueTypes;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using GameFinder.StoreHandlers.EGS;
using MWSManager.Services;
using System.Threading.Tasks;
using Serilog;

namespace MWSManager.ViewModels;

internal class GameDefinition
{
    public uint? SteamId;
    public uint? MWSId;
    public string? EpicId;
    public string? ClassName;
    public string? Icon;
    public string[]? ModDirs;

    public dynamic? ExtraData;
}

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<GamePageViewModel> Games { get; set; } = [];
    public ObservableCollection<PageViewModel> Pages { get; set; } = [];
    public ObservableCollection<PageViewModel> OtherPages { get; set; } = [];

    public DownloadsPageViewModel Downloads { get; }

    [ObservableProperty]
    private PageViewModel currentPage;

    [ObservableProperty]
    private GamePageViewModel? currentGame;

    [ObservableProperty]
    private PageViewModel? currentOtherPage;

    public MainWindowViewModel()
    {
        Downloads = new DownloadsPageViewModel() { 
            Window = this
        };

        OtherPages.Add(Downloads);
        OtherPages.Add(new SettingsPageViewModel());

        var steamHandler = new SteamHandler(FileSystem.Shared, OperatingSystem.IsWindows() ? WindowsRegistry.Shared : null);
        var epicHandler = new EGSHandler(WindowsRegistry.Shared, FileSystem.Shared);

        var fs = new StreamReader(AssetLoader.Open(new Uri("avares://MWSManager/Assets/games.json")));
        var jsonConfig = fs.ReadToEnd();
        var data = JsonConvert.DeserializeObject<Dictionary<string, GameDefinition>>(jsonConfig);

        Log.Information("Loading Games");

        if (data != null)
        {
            foreach(var kv in data)
            {
                var gameDef = kv.Value;

                IGame? game = null;
                string gamePath = "";
                string gameName = "";

                var errs = new ErrorMessage[10];

                if (gameDef.SteamId != null)
                {
                    var steamGame = steamHandler.FindOneGameById(AppId.From((uint)gameDef.SteamId), out errs);
                    if (steamGame != null)
                    {
                        gamePath = steamGame.Path.GetFullPath();
                        gameName = steamGame.Name;
                    }

                    game = steamGame;
                }

                // TODO: show warning if the user has both/how do we handle it?
                if (gameDef.EpicId != null && game == null)
                {
                    var egsGame = epicHandler.FindOneGameById(EGSGameId.From(gameDef.EpicId), out errs);
                    if (egsGame != null)
                    {
                        gamePath = egsGame.InstallLocation.GetFullPath();
                        gameName = egsGame.DisplayName; // Why the fuck is it inconsistent???
                    }

                    game = egsGame;
                }

                if (game == null) { 
                    continue;
                }

                Game? gameObj = null;
                if (gameDef.ClassName != null)
                {
                    var t = Type.GetType("MWSManager.Models." + gameDef.ClassName);
                    if (t != null)
                    {
                        gameObj = (Game?)Activator.CreateInstance(t, gameName, gamePath, gameDef.ExtraData);
                    } else
                    {
                        Log.Warning($"couldn't find {gameDef.ClassName}");
                    }
                }
                else
                {
                    gameObj = new Game(gameName, gamePath, gameDef.ExtraData);
                }


                if (gameObj != null)
                {
                    gameObj.Thumbnail = gameDef.Icon;
                    gameObj.MWSId = gameDef.MWSId;

                    if (gameDef.ModDirs != null)
                    {
                        gameObj.ModDirs.AddRange(gameDef.ModDirs);
                    }
                    var gv = new GamePageViewModel(gameObj);
                    Games.Add(gv);
                    Pages.Add(gv);
                }
                else
                {
                    Log.Error("Couldn't Create Game {0}", kv.Key);
                }
            }
        }

        if (Games.Count > 0)
        {
            CurrentPage = Games[0];
        }
    }

    partial void OnCurrentPageChanging(PageViewModel value)
    {
        if (value != null && value is GamePageViewModel)
        {
            var game = (GamePageViewModel)value;
            game.LoadMods();
        }
    }

    partial void OnCurrentGameChanged(GamePageViewModel? value)
    {
        if (value != null)
        {
            CurrentOtherPage = null;
            CurrentPage = value;
        }
    }
    partial void OnCurrentOtherPageChanged(PageViewModel? value)
    {
        if (value != null)
        {
            CurrentGame = null;
            CurrentPage = value;
        }
    }

    // 
    public async Task TryInstallMod(int gameId, ModInstall install)
    {
        await Task.Run(() =>
        {
            foreach(var gameVm in Games) {
                if (gameVm.Game.MWSId == gameId)
                {
                    gameVm.TryInstallMod(install);
                }
            }
        });
    }
}

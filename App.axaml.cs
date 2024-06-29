using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GameFinder.RegistryUtils;
using GameFinder.StoreHandlers.Steam;
using MWSManager.Models;
using MWSManager.ViewModels;
using MWSManager.Views;
using NexusMods.Paths;
using System;
using System.Diagnostics;
using HotAvalonia;
using URIScheme;
using System.Reflection;
using System.Threading;
using System.Text.RegularExpressions;
using System.Linq;
using MWSManager.Services;
using Avalonia.Threading;
using Avalonia.Controls;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Serilog;

namespace MWSManager;

public partial class App : Application
{
    [DllImport("Kernel32")]
    public static extern void AllocConsole();

    [DllImport("Kernel32")]
    public static extern void FreeConsole();

    public override void Initialize()
    {
        this.EnableHotReload(); // Ensure this line **precedes** `AvaloniaXamlLoader.Load(this);

        AllocConsole();

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        Log.Information("Loading App");

        var service = URISchemeServiceFactory.GetURISchemeSerivce("MWSManager-mm", "URL:MWSManager Protcol", Process.GetCurrentProcess().MainModule.FileName);
        if (service.CheckAny())
        {
            service.Delete();
        }

        service.Set();

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();

        string[] args = Environment.GetCommandLineArgs();
        if (args.Length > 2)
        {
            if (args[1] != null)
            {
                OnMessage(args[1]);
            }

        }
    }

    public void OnMessage(string message)
    {
        if (message != null) {
            var match = Regex.Match(message, @"MWSManager-mm://mws/install/(\d+)");
            if (match.Groups[1].Value != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    MWSService.TryInstallingFile(Int32.Parse(match.Groups[1].Value));
                });
                
            }
        }
    }
}
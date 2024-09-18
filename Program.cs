using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using MWSManager.Views;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.MaterialDesign;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MWSManager;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        bool ret;
        new Mutex(true, "MWSManager-MM", out ret);

        if (!ret)
        {
            // Write to the single instance
            NamedPipeClientStream client = new NamedPipeClientStream("MWSManager-ModManager");
            client.Connect(100);

            if (client.IsConnected)
            {
                using var writer = new BinaryWriter(client);
                writer.Write(string.Join(" ", args));
            }

            // We sent what we are supposed to, exit
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.Shutdown();
            }

            return;
        }

        _ = CheckForMessage();

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        } 
        catch (Exception e)
        {
            Log.Fatal(e, "Something very bad happened");
        } 
        finally
        {
            Log.CloseAndFlush();
        }

    }

    async static Task CheckForMessage()
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

        while (await timer.WaitForNextTickAsync())
        {
            using var server = new NamedPipeServerStream("MWSManager-ModManager");
            server.WaitForConnection();

            using var reader = new BinaryReader(server);
            try
            {
                string arguments = reader.ReadString();
                if (Application.Current != null)
                {
                    App app = (App)Application.Current;
                    app.OnMessage(arguments);
                }
            }
            catch {}
            server.Disconnect();
        } 
    }

    private void HandleIncomingArguments(string args)
    {
        Trace.WriteLine(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current.Register<MaterialDesignIconProvider>();

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }
}

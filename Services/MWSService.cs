using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using MWSManager.Models;
using MWSManager.ViewModels;
using MWSManager.Views;
using Newtonsoft.Json;
using Serilog;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace MWSManager.Services;

public class MWSFile
{
    public int id { get; init; }
    public string mod_id { get; init; }
    public string name { get; init; }
    public string file { get; init; }
    public string type { get; init; }
    public string download_url { get; init; }
}

public class MWSMod
{
    public int game_id { get; init; }

    public string name { get; init; }
}

public class MWSService
{
    public static async Task TryInstallingFile(int fileId)
    {
        var client = Utils.GetHTTPClient();

        //TODO: DON'T HARDCODE THIS
        var response = await client.GetStringAsync($"https://dev-api.modworkshop.net/files/{fileId}");
        if (response == null)
        {
            return;
        }

        try
        {
            var fileData = JsonConvert.DeserializeObject<MWSFile>(response);
            var ext = Path.GetExtension(fileData.file); // The extension of the file. Not necessarily truthy.

            var modResponse = await client.GetStringAsync($"https://dev-api.modworkshop.net/mods/{fileData.mod_id}");
            if (modResponse == null)
            {
                return;
            }

            var mod = JsonConvert.DeserializeObject<MWSMod>(modResponse);
            var app = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
            var win = (MainWindowViewModel)app.MainWindow.DataContext;

            using var fileResponse = await client.GetAsync(fileData.download_url, HttpCompletionOption.ResponseHeadersRead);
            fileResponse.EnsureSuccessStatusCode();
            using var modStream = await fileResponse.Content.ReadAsStreamAsync();
            //using var fileStream = new FileStream(Path.Combine(Path.GetTempPath(), "MWSManager", Path.GetTempFileName()), FileMode.CreateNew);
            using var memStream = new MemoryStream();

            long? totalBytes = fileResponse.Content.Headers.ContentLength;
            var buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead = 0;

            var download = win.Downloads.AddDownload(mod.name, fileData.name);

            while ((bytesRead = await modStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await memStream.WriteAsync(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;

                download.Progress = (int)(100 * ((double)totalBytesRead / totalBytes));
            }

            memStream.Position = 0;

            download.ShowProgressPercent = false;
            download.ProgressText = "Installing...";
            await win.TryInstallMod(mod.game_id, new FileData
            {
                Name = fileData.file,
                Type = fileData.file.Split(".").Last(),
                Stream = memStream
            });

            download.ProgressText = "Done!";

            Log.Information("Closing stream in TryInstallingFile");
        }
        catch (Exception e)
        {
            Log.Error("Failed installing mod file in TryInstallingFile {0}", e.Message);
        }
    }
}

﻿using MWSManager.Models;
using MWSManager.Models.Providers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MWSManager.Services;

public class UpdatesService
{
    public ObservableCollection<ModUpdate> Updates { get; private set; } = [];

    private List<Provider> Providers = [];

    bool CheckingForUpdates = false;

    private UpdatesService() {
        RegisterProvider(new ModWorkshop());
        RegisterProvider(new ModWorkshopFile());
    }

    private static UpdatesService? instance = null;
    public static UpdatesService Instance
    {
        get
        {
            if (instance == null)
                instance = new UpdatesService();

            return instance;
        }
    }

    public void InitialCheckForUpdates()
    {
        var t = new Thread(AutoCheckForUpdates);
        t.Start();
    }

    public void RegisterProvider(Provider provider)
    {
        Providers.Add(provider);
    }

    public void AddUpdate(ModUpdate update)
    {
        Log.Information(update.Id + " , " + update.Provider);
        Updates.Add(update);
    }

    public void RemoveUpdate(ModUpdate update)
    {
        Updates.Remove(update);
    }

    public void DownloadAndInstall(ModUpdate update)
    {
        foreach (var provider in Providers)
        {
            if (provider.Name == update.Provider)
            {
                Log.Information("Initating update for {0}", update.Id);
                _ = provider.StartModUpdate(update);
            }
        }
    }

    public void URISchemeHandle(string providerName, string action, string id)
    {
        foreach (var provider in Providers)
        {
            if (provider.Name == providerName)
            {
                _= provider.URISchemeHandle(action, id);
            }
        }
    }

    private async void AutoCheckForUpdates()
    {
        while(true)
        {
            await CheckForUpdates();
            Thread.Sleep(SettingsService.Data.AutoCheckT * 60 * 60 * 1000); // Default 6 hours
        }
    }

    /// <summary>
    /// Initiates a check for updates. Will not check if a check is in progress
    /// </summary>
    public async Task CheckForUpdates()
    {
        if (CheckingForUpdates)
        {
            Log.Warning("Already checking for updates!");
            return;
        }

        Log.Information("Checking for updates...");

        CheckingForUpdates = true;
        Dictionary<string, List<ModUpdate>> providerUpdates = [];
        foreach (var provider in Providers)
        {
            providerUpdates.Add(provider.Name, []);
        }

        foreach (var update in Updates)
        {
            Log.Information(update.Provider);
            providerUpdates[update.Provider]?.Add(update);
        }

        foreach (var provider in Providers)
        {
            await provider.CheckMultipleUpdates(providerUpdates[provider.Name]);
        }

        Log.Information("Checking for updates...Done");

        CheckingForUpdates = false;
    }
}

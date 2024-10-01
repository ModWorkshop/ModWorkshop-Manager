using MWSManager.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWSManager.Services;

public class SettingsService
{
    private Settings? data;

    private static SettingsService? instance = null;
    public static SettingsService Instance
    {
        get
        {
            if (instance == null)
                instance = new SettingsService();

            return instance ?? new SettingsService();
        }
    }

    public static Settings Data => Instance.data;

    public SettingsService()
    {
        Load();
    }

    /// <summary>
    /// Loads settings from disk. If fails, resets the settings.
    /// </summary>
    public void Load()
    {
        Log.Information("Loading settings from disk...");

        data = null;

        try
        {
            if (File.Exists("settings.json"))
            {
                data = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
            }
        }
        catch (Exception)
        {
            Log.Error("Failed reading settings file! Loading default settings...");
        }

        data ??= new Settings();
    }

    /// <summary>
    /// Saves the currently loaded settings into the disk
    /// </summary>
    public void Save()
    {
        Log.Information("Saving settings to disk...");
        File.WriteAllText("settings.json", JsonConvert.SerializeObject(data, Formatting.Indented));
    }
}

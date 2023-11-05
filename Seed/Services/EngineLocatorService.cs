using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Seed.Models;

namespace Seed.Services;

public class EngineLocatorService: IEngineLocatorService
{
    public const string AppName = "SeedLauncher";
    public const string EnginesSaveFile = "Engines.json";

    private List<Engine> _engines = new();

    public EngineLocatorService()
    {
        LocateEngines();
    }

    private void LocateEngines()
    {
        var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);
        var enginesFile = Path.Combine(dataFolder, EnginesSaveFile);
        if (!File.Exists(enginesFile))
            return;
        var json = File.ReadAllText(enginesFile);
        if (string.IsNullOrWhiteSpace(json))
            return;
 
        try
        {
            var engines = JsonSerializer.Deserialize<List<Engine>>(json);
            if (engines is null)
            {
                // TODO: Log this
                return;
            }

            _engines = engines;
        }
        catch (JsonException je)
        {
            Console.WriteLine($"Exception while attempting to deserialize engine info: {je}");
        }       
    }
    
    public List<Engine> GetInstalledEngines()
    {
        return _engines;
    }
}
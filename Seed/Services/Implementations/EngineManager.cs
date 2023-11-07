using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Seed.Models;

namespace Seed.Services;

public class EngineManager: IEngineManager
{
    private List<Engine> _engines = new();
    
    public EngineManager()
    {
        LocateEngines();
        ValidateEngines();
    }
    
    public List<Engine> GetInstalledEngines()
    {
        return _engines;
    }

    public void AddEngine(Engine engine)
    {
        _engines.Add(engine);
        Save();
    }
    
    private void LocateEngines()
    {
        var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Globals.AppName);
        var enginesFile = Path.Combine(dataFolder, Globals.EnginesSaveFileName);
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

    private void ValidateEngines()
    {
        foreach (var engine in _engines.ToList())
        {
            if (!engine.ValidateInstallation())
            {
                _engines.Remove(engine);
                continue;
            }

            foreach (var package in engine.InstalledPackages.ToList())
            {
                if (!package.ValidateInstallation())
                    engine.InstalledPackages.Remove(package);
            }
        }
    }

    private void Save()
    {
        var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Globals.AppName);
        var enginesFile = Path.Combine(dataFolder, Globals.EnginesSaveFileName);

        using var file = new FileStream(enginesFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
 
        JsonSerializer.Serialize(file, _engines);
    }
}
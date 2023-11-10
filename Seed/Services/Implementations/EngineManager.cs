using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Seed.Models;

namespace Seed.Services.Implementations;

public class EngineManager : IEngineManager
{
    public ObservableCollection<Engine> Engines { get; private set; } = new();

    public EngineManager()
    {
        LocateEngines();
        ValidateEngines();
    }

    public void AddEngine(Engine engine)
    {
        Engines.Add(engine);
        Save();
    }

    public void DeleteEngine(Engine engine)
    {
        if (Engines.Contains(engine))
        {
            Engines.Remove(engine);
        }

        Save();
    }

    private void LocateEngines()
    {
        var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Globals.AppName);
        var enginesFile = Path.Combine(dataFolder, Globals.EnginesSaveFileName);
        if (!File.Exists(enginesFile))
            return;
        var json = File.ReadAllText(enginesFile);
        if (string.IsNullOrWhiteSpace(json))
            return;

        try
        {
            var engines = JsonSerializer.Deserialize<List<Engine>>(json, new JsonSerializerOptions
            {
                TypeInfoResolver = EngineGenerationContext.Default,
            });
            if (engines is null)
            {
                // TODO: Log this
                return;
            }

            Engines = new ObservableCollection<Engine>(engines);
        }
        catch (JsonException je)
        {
            Console.WriteLine($"Exception while attempting to deserialize engine info: {je}");
        }
    }

    private void ValidateEngines()
    {
        foreach (var engine in Engines.ToList())
        {
            if (!engine.ValidateInstallation())
            {
                Engines.Remove(engine);
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
        var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Globals.AppName);
        var enginesFile = Path.Combine(dataFolder, Globals.EnginesSaveFileName);

        using var file = new FileStream(enginesFile, FileMode.Create, FileAccess.Write, FileShare.None);

        JsonSerializer.Serialize(file, Engines.ToList(), new JsonSerializerOptions
        {
            TypeInfoResolver = EngineGenerationContext.Default,
            WriteIndented = true
        });
    }
}
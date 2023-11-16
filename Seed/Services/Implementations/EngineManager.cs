using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;
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

    /// <inheritdoc />
    public void AddEngine(Engine engine)
    {
        Engines.Add(engine);
        Save();
    }

    /// <inheritdoc />
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

    static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        var dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (var subDir in dirs)
            {
                var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}
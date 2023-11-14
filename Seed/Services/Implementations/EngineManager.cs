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

    public async void CreateProject(Project newProject, Project template)
    {
        var path = new Uri(template.Path);
        if (path.IsFile && path.AbsolutePath != "/" && !Directory.Exists(template.Path))
        {
            // TODO: Notify user
            return;
        }


        var projectManager = App.Current.Services.GetService<IProjectManager>();

        // This is where template will be located.

        if (path is { Scheme: "avares" })
        {
            var newProjectParentDir = Directory.GetParent(newProject.Path)!.FullName;

            // var stream = AssetLoader.Open(path);
            var stream = new FileStream("/home/minebill/Downloads/BasicScene.zip", FileMode.Open, FileAccess.Read);
            // Console.WriteLine($"Stream has {stream.Length} bytes.\n\tPosition: {stream.Position}");
            // Console.WriteLine($"\tReading 1 byte: {stream.ReadByte()}");
            await ZipHelpers.ExtractToDirectoryAsync(stream, newProjectParentDir, new Progress<float>());

            var unzippedPath = Path.Combine(newProjectParentDir, template.Name);
            var flaxproj = Path.Combine(unzippedPath, template.Name) + ".flaxproj";
            var jsonText = await File.ReadAllTextAsync(flaxproj);
            var json = JsonNode.Parse(jsonText);
            if (json != null)
                json["Name"] = newProject.Name;

            await using var writer = new Utf8JsonWriter(new FileStream(flaxproj, FileMode.Create), new JsonWriterOptions
            {
                Indented = true
            });
            json?.WriteTo(writer);

            File.Move(flaxproj, Path.Combine(unzippedPath, newProject.Name + ".flaxproj"));
            Directory.Move(unzippedPath, newProject.Path);

            projectManager?.AddProject(newProject);
        }
        else if (path.IsFile && path.AbsolutePath != "/")
        {
            CopyDirectory(template.Path, newProject.Path, true);

            // We prefer to delete unneeded folders instead of only copying the needed ones
            // because we don't know if this template projects has extra folders that are needed.
            Directory.Delete(Path.Combine(newProject.Path, "Cache"), recursive: true);
            Directory.Delete(Path.Combine(newProject.Path, "Binaries"), recursive: true);

            var flaxproj = Path.Combine(newProject.Path, template.Name) + ".flaxproj";
            var jsonText = await File.ReadAllTextAsync(flaxproj);
            var json = JsonNode.Parse(jsonText);
            if (json != null)
                json["Name"] = newProject.Name;

            await using var writer = new Utf8JsonWriter(new FileStream(flaxproj, FileMode.Create), new JsonWriterOptions
            {
                Indented = true
            });
            json?.WriteTo(writer);

            File.Move(flaxproj, Path.Combine(newProject.Path, newProject.Name + ".flaxproj"));

            projectManager?.AddProject(newProject);
        }
        else if (path.AbsolutePath == "/")
        {
            var engine = Engines.First(x => x.Version == newProject.EngineVersion);
            // No path was given. Tell Flax to create a completely blank project.
            var info = new ProcessStartInfo
            {
                FileName = engine.GetExecutablePath("Release"),
                Arguments = $"-new -project \"{newProject.Path}\""
            };

            // TODO: handle
            var process = Process.Start(info);
            if (process is null)
                throw new Exception("Failed to create Flax process while creating a new project.");

            await process.WaitForExitAsync();

            projectManager?.AddProject(newProject);
        }
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
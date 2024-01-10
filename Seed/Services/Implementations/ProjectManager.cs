using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using NLog;
using Seed.Models;

namespace Seed.Services.Implementations;

public class ProjectManager : IProjectManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly IEngineManager _engineManager;

    public ObservableCollection<Project> Projects { get; private set; } = new();

    public ProjectManager(IEngineManager engineManager)
    {
        _engineManager = engineManager;

        LoadProjects();
        ValidateProjects();
    }

    public void AddProject(Project project)
    {
        // NOTE(minebill): Can _engineManager.Engines be empty at this point?

        foreach (var engine in _engineManager.Engines)
        {
            if (engine.Version != project.EngineVersion) continue;

            project.Engine = engine;
            Projects.Add(project);
            Save();
            break;
        }
    }

    public void RemoveProject(Project project)
    {
        Projects.Remove(project);
        Save();
    }

    public void RunProject(Project project)
    {
        if (_engineManager.Engines.Count == 0)
        {
            return;
        }

        var engine = _engineManager.Engines.First(x => x.Version == project.EngineVersion);

        if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
        {
            var info = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments =
                    $" -c \"setsid {engine.GetExecutablePath(engine.PreferredConfiguration)} -project \"{Path.GetFullPath(project.Path)}\" \"",
                CreateNoWindow = true,
            };

            Process.Start(info);
        }
        else if (OperatingSystem.IsWindows())
        {
            var info = new ProcessStartInfo
            {
                FileName = engine.GetExecutablePath(engine.PreferredConfiguration),
                Arguments = $"-project \"{Path.GetFullPath(project.Path)}\"",
            };

            Process.Start(info);
        }
    }

    public void ClearCache(Project project)
    {
        Logger.Info($"Deleting cache folder for {project.Name}");
        var cache = Path.Combine(project.Path, "Cache");
        if (Directory.Exists(cache))
        {
            Logger.Info("Deleted cache folder.");
            Directory.Delete(cache, true);
        }
    }

    private void LoadProjects()
    {
        var configFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Globals.AppName);
        var saveFile = Path.Combine(configFolder, Globals.ProjectsSaveFileName);
        if (!File.Exists(saveFile))
            return;

        var json = File.ReadAllText(saveFile);
        if (string.IsNullOrWhiteSpace(json))
            return;
        try
        {
            // TODO: Handle this properly
            var projects = JsonSerializer.Deserialize<List<Project>?>(json, new JsonSerializerOptions
            {
                TypeInfoResolver = ProjectGenerationContext.Default
            });
            if (projects is null)
            {
                Logger.Warn("Json serializer returned a null project.");
                return;
            }

            Projects = new ObservableCollection<Project>(projects);
            if (_engineManager.Engines.Count > 0)
            {
                foreach (var project in Projects)
                {
                    // BUG: This will fail if you remove all engines but have some projects.
                    project.Engine = _engineManager.Engines.First(x => x.Version == project.EngineVersion);
                }
            }
        }
        catch (JsonException je)
        {
            Logger.Error($"Exception while attempting to deserialize project info: {je}");
        }
    }

    private void ValidateProjects()
    {
        foreach (var project in Projects.ToList())
        {
            if (!project.ValidateInstallation())
                Projects.Remove(project);
        }
    }

    public void Save()
    {
        var configFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Globals.AppName);
        var saveFile = Path.Combine(configFolder, Globals.ProjectsSaveFileName);

        using var file = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);

        JsonSerializer.Serialize(file, Projects.ToList(), new JsonSerializerOptions
        {
            TypeInfoResolver = ProjectGenerationContext.Default,
            WriteIndented = true
        });
    }
}
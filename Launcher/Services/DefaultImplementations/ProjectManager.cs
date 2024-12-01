using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Launcher.DataModels;
using LibGit2Sharp;
using NLog;
using Version = System.Version;

namespace Launcher.Services.DefaultImplementations;

public class ProjectManager : IProjectManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly IEngineManager _engineManager;
    private readonly IDownloadManager _downloadManager;

    public event IProjectManager.SaveEvent? OnSaved;
    public ObservableCollection<Project> Projects { get; private set; } = new();

    public ProjectManager(IEngineManager engineManager, IDownloadManager downloadManager)
    {
        _engineManager = engineManager;
        _downloadManager = downloadManager;

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

    public Project? ParseProject(string path)
    {
        if (!Path.Exists(path)) return null;

        var projFile = File.ReadAllText(path);
        var root = JsonNode.Parse(projFile);
        if (root is null) return null;
        if (!root.AsObject().ContainsKey("MinEngineVersion"))
            return null;
        var ok = Version.TryParse(root["MinEngineVersion"]!.ToString(), out var version);
        if (!ok) return null;
        var engines = _engineManager.Engines.OrderBy(e => e.Version);
        var engine = engines.FirstOrDefault(e => e.Version.CompareTo(new NormalVersion(version!)) >= 0);
        if (engine is null) return null;

        return new Project(root["Name"]!.ToString(), Directory.GetParent(path)!.FullName, path, engine.Version);
    }

    public Task<Project?> AddProjectFromGitRepo(string repoUrl, string destination)
    {
        return Task.Run(() =>
        {
            var download = new DownloadEntry();
            _downloadManager.AddDownload(download);
            download.Title = "Cloning repo";
            IProgress<float> progress = download.Progress;

            var options = new CloneOptions
            {
                RecurseSubmodules = true
            };
            options.OnCheckoutProgress += (path, steps, totalSteps) =>
            {
                download.CurrentAction = "Checking out";
                progress.Report(steps / (float)totalSteps);
            };
            options.FetchOptions.OnTransferProgress += transferProgress =>
            {
                download.CurrentAction = $"Received {transferProgress.ReceivedBytes} bytes";
                progress.Report(transferProgress.ReceivedObjects / (float)transferProgress.TotalObjects);
                return true;
            };
            Repository.Clone(repoUrl, destination, options);
            _downloadManager.RemoveDownload(download);

            var projectFiles = Directory.GetFiles(destination).Where(f => f.EndsWith("flaxproj")).ToArray();
            if (projectFiles.Length > 1) return null;

            return ParseProject(projectFiles[0]);
        });
    }

    public void RemoveProject(Project project)
    {
        Projects.Remove(project);
        Save();
    }

    public void RunProject(Project project)
    {
        if (_engineManager.Engines.Count == 0)
            return;

        var engine = _engineManager.Engines.FirstOrDefault(x => x.Version == project.EngineVersion);
        if (engine is null) return;

        var enginePath = engine.GetExecutablePath(engine.PreferredConfiguration);

        if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
        {
            var fullProjectPath = Path.GetFullPath(project.Path)
                .Replace(@"(", @"\(")
                .Replace(@")", @"\)");

            var arguments =
                @$" -c ""setsid {engine.GetExecutablePath(engine.PreferredConfiguration)} -project \\\""{fullProjectPath}\\\"" {project.ProjectArguments ?? string.Empty} """;

            var info = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments = arguments,
                CreateNoWindow = true,
            };

            Process.Start(info);
        }
        else if (OperatingSystem.IsWindows())
        {
            var info = new ProcessStartInfo
            {
                FileName = enginePath,
                Arguments = $"-project \"{Path.GetFullPath(project.Path)}\" {project.ProjectArguments ?? string.Empty}",
            };

            Process.Start(info);
        }

        project.LastOpenedTime = DateTime.Now;
        Save();
    }

    public void ClearCache(Project project)
    {
        Logger.Info("Deleting cache folder for {ProjectName}", project.Name);
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
            var projects = JsonSerializer.Deserialize(json, ProjectListGenerationContext.Default.ListProject);
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
                    try
                    {
                        project.Engine = _engineManager.Engines.First(x => x.Version == project.EngineVersion);
                    }
                    catch (InvalidOperationException)
                    {
                        // NOTE: We do not perform any kind of automatic increase in the project version.
                        // If a new patch gets released, the user must explicitly set their project to use
                        // that engine version.
                        project.Engine = null;
                    }
                }
            }
        }
        catch (JsonException je)
        {
            Logger.Error(je, "Exception while attempting to deserialize project info");
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

        JsonSerializer.Serialize(file, Projects.ToList(), ProjectListGenerationContext.Default.ListProject);

        OnSaved?.Invoke();
    }
}
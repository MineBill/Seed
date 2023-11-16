using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NLog;
using Seed.Models;
using Seed.ViewModels;

namespace Seed.Services.Implementations;

public class EngineDownloaderService : IEngineDownloaderService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public const string ApiUrl = "https://api.flaxengine.com/launcher/engine";

    public const string GithubWorkflowApiUrl =
        "https://api.github.com/repos/FlaxEngine/FlaxEngine/actions/workflows/cd.yml/runs?per_page=3";

    public const string GithubArtifactsApiUrl =
        "https://api.github.com/repos/FlaxEngine/FlaxEngine/actions/workflows/cd.yml/runs?per_page=3";

    public event Action<string> ActionChanged;

    private HttpClient _client = new();
    private Progress<float> _progress = new();
    public Progress<float> Progress => _progress;
    private string _currentAction;

    public string CurrentAction
    {
        get => _currentAction;
        private set
        {
            _currentAction = value;
            ActionChanged?.Invoke(value);
        }
    }

    public EngineDownloaderService()
    {
    }

    public async Task<List<RemoteEngine>?> GetAvailableVersions()
    {
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Add("User-Agent", "Seed Launcher for Flax");

        var json = await _client.GetStringAsync(ApiUrl);
        try
        {
            var tree = JsonNode.Parse(json);
            if (tree is null)
                return null;

            var engines = tree["versions"].Deserialize<List<RemoteEngine>>();
            return engines;
        }
        catch (JsonException je)
        {
            Logger.Error(je, "Failed to read flax api.");
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Exception",
                $"An exception occured while deserializing information from the Flax API. It's possible the API has changed. Please make an issue at {Globals.RepoUrl}.",
                icon: Icon.Error);
            await box.ShowWindowDialogAsync(App.Current.MainWindow);
            return null;
        }
    }

    public async Task<List<Workflow>?> GetGithubWorkflows()
    {
        var prefs = App.Current.Services.GetService<IPreferencesSaver>()!;
        if (prefs.Preferences.GithubAccessToken is null)
            return null;
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Add("User-Agent", "Seed Launcher for Flax");
        _client.DefaultRequestHeaders.Add("Bearer", prefs.Preferences.GithubAccessToken);
        _client.Timeout = TimeSpan.FromSeconds(30);

        var list = new List<Workflow>();
        var json = await _client.GetStringAsync(GithubWorkflowApiUrl);
        try
        {
            var workflows = JsonNode.Parse(json);
            if (workflows is null)
                return null;

            foreach (var workflowNode in workflows["workflow_runs"]!.AsArray())
            {
                var workflow = workflowNode?.Deserialize<Workflow>();
                if (workflow is null || !workflow.IsValid)
                    continue;

                workflow.Artifacts = new List<Artifact>();

                var artifactsJson = await _client.GetStringAsync(
                    $"https://api.github.com/repos/FlaxEngine/FlaxEngine/actions/runs/{workflow.Id}/artifacts");

                var artifacts = JsonNode.Parse(artifactsJson);
                foreach (var artifactNode in artifacts["artifacts"].AsArray())
                {
                    var artifact = artifactNode.Deserialize<Artifact>();
                    if (artifact is null)
                        continue;

                    artifact.CommitHash = workflow.CommitHash;

                    var split = artifact.Name.Split('-');
                    var (os, type) = (split[0], split[1]);

                    artifact.IsEditor = string.Equals(type, "Editor", StringComparison.Ordinal);

                    artifact.TargetPath = os switch
                    {
                        "Linux" => Path.Combine("Source", "Platforms", "Linux"),
                        "Windows" => Path.Combine("Source", "Platforms", "Windows"),
                        "Mac" => Path.Combine("Source", "Platforms", "Mac"),
                        _ => string.Empty
                    };

                    artifact.OperatingSystem = os switch
                    {
                        "Linux" => OSPlatform.Linux,
                        "Windows" => OSPlatform.Windows,
                        "Mac" => OSPlatform.OSX,
                        _ => throw new UnreachableException("Unknown os in artifact name")
                    };

                    workflow.Artifacts.Add(artifact);
                }

                list.Add(workflow);
            }
        }
        catch (JsonException je)
        {
            Logger.Error(je, "Failed to read github api.");
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Exception",
                $"An exception occured while deserializing information from the Flax API. It's possible the API has changed. Please make an issue at {Globals.RepoUrl}.",
                icon: Icon.Error);
            await box.ShowWindowDialogAsync(App.Current.MainWindow);
            return null;
        }

        return list;
    }

    public async Task<Engine> DownloadVersion(RemoteEngine engine, List<RemotePackage> platformTools,
        string installFolderPath)
    {
        var tempEditorFile = Path.GetTempFileName();
        CurrentAction = $"Downloading {engine.Name}";
        var editorUrl = engine.GetEditorPackage().EditorUrl;
        await using (var file = new FileStream(tempEditorFile, FileMode.Create, FileAccess.Write, FileShare.None))
            await _client.DownloadDataAsync(editorUrl, file, _progress);
        // await DownloadFile(editorUrl, tempEditorFile);
        // create sub folder for this engine installation
        var editorInstallFolder = Path.Combine(installFolderPath, engine.Name);

        // TODO: Check for errors
        // ZipFile.ExtractToDirectory(tempEditorFile, editorInstallFolder);
        CurrentAction = "Extracting editor";
        await ZipHelpers.ExtractToDirectoryAsync(tempEditorFile, editorInstallFolder, _progress);

        var installedPackages = new List<Package>(platformTools.Count);
        foreach (var tools in platformTools)
        {
            CurrentAction = $"Downloading platform tools for {tools.Name}";
            var tmpFile = Path.GetTempFileName();
            await using (var file = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.None))
                await _client.DownloadDataAsync(tools.Url, file, _progress);

            var installFolder = Path.Combine(editorInstallFolder, tools.TargetPath);
            CurrentAction = $"Extracting {tools.Name}";
            await ZipHelpers.ExtractToDirectoryAsync(tmpFile, installFolder, _progress);

            installedPackages.Add(new Package(tools.Name, installFolder));
        }

        CurrentAction = "Done!";

        return new Engine
        {
            Name = engine.Name,
            Path = editorInstallFolder,
            Version = engine.Version,
            InstalledPackages = installedPackages
        };
    }

    public async Task<Engine> DownloadFromWorkflow(Workflow workflow, List<Artifact> platformTools,
        string installFolderPath)
    {
        var prefs = App.Current.Services.GetService<IPreferencesSaver>()!;
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Add("User-Agent", "Seed Launcher for Flax");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", prefs.Preferences.GithubAccessToken);

        var tempEditorFile = Path.GetTempFileName();
        CurrentAction = $"Downloading commit {workflow.CommitHash}";
        var editorUrl = workflow.EditorArtifact.DownloadUrl;
        await using (var file = new FileStream(tempEditorFile, FileMode.Create, FileAccess.Write, FileShare.None))
            await _client.DownloadDataAsync(editorUrl, file, _progress,
                contentLength: workflow.EditorArtifact.SizeInBytes);

        var editorInstallFolder = Path.Combine(installFolderPath, workflow.CommitHash);

        // TODO: Check for errors
        CurrentAction = "Extracting artifact";

        // Because the artifacts are zips inside a zip, we need to extract the nested zip first.
        // To do that, create a folder named after the commit and extract the first zip there.
        // Then extract that zip to the destination folder.
        var nestedZipPath = Path.Combine(Path.GetTempPath(), workflow.CommitHash);
        await ZipHelpers.ExtractToDirectoryAsync(tempEditorFile, nestedZipPath, _progress);

        CurrentAction = "Extracting editor";
        // TODO: This should return at least one but maybe check that.
        var nestedZip = Directory.GetFiles(nestedZipPath)[0];
        await ZipHelpers.ExtractToDirectoryAsync(nestedZip, editorInstallFolder, _progress);

        var installedPackages = new List<Package>(platformTools.Count);
        foreach (var tools in platformTools)
        {
            CurrentAction = $"Downloading platform tools for {tools.Name}";
            var tmpFile = Path.GetTempFileName();
            await using (var file = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.None))
                await _client.DownloadDataAsync(tools.DownloadUrl, file, _progress, contentLength: tools.SizeInBytes);

            var installFolder = Path.Combine(editorInstallFolder, tools.TargetPath);
            CurrentAction = $"Extracting {tools.Name} artifact";

            var nestedPlatformZipPath = Path.GetRandomFileName();
            await ZipHelpers.ExtractToDirectoryAsync(tmpFile, nestedPlatformZipPath, _progress);

            CurrentAction = $"Extracting {tools.Name}";
            var nestedPlatformZip = Directory.GetFiles(nestedPlatformZipPath)[0];
            await ZipHelpers.ExtractToDirectoryAsync(nestedPlatformZip, installFolder, _progress);

            installedPackages.Add(new Package(tools.Name, installFolder));
        }

        CurrentAction = "Done!";

        return new Engine
        {
            Name = workflow.CommitHash,
            Path = editorInstallFolder,
            Version = new Version(0, 0, 4269),
            InstalledPackages = installedPackages
        };
    }
}
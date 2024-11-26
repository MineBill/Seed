using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Launcher.DataModels;
// using MsBox.Avalonia;
// using MsBox.Avalonia.Enums;
using NLog;

namespace Launcher.Services.DefaultImplementations;

public class EngineDownloader(IPreferencesManager preferencesManager) : IEngineDownloader
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public const string FlaxLauncherApiUrl = "https://api.flaxengine.com/launcher/engine";

    public const string GithubWorkflowApiUrl =
        "https://api.github.com/repos/FlaxEngine/FlaxEngine/actions/workflows/cd.yml/runs?per_page=3";

    public event Action<string>? ActionChanged;
    public event Action? DownloadStarted;
    public event Action? DownloadFinished;

    private readonly HttpClient _client = new();
    public Progress<float> Progress { get; } = new();

    private string _currentAction = string.Empty;
    private CancellationTokenSource _cancellationTokenSource = new();

    public string CurrentAction
    {
        get => _currentAction;
        private set
        {
            _currentAction = value;
            ActionChanged?.Invoke(value);
        }
    }

    /// <inheritdoc />
    public async Task<List<RemoteEngine>?> GetAvailableVersions()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, FlaxLauncherApiUrl);
        // _client.DefaultRequestHeaders.Accept.Clear();
        // _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        // _client.DefaultRequestHeaders.Add("User-Agent", "Seed Launcher for Flax");
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("User-Agent", "Seed Launcher for Flax");

        var response = await _client.SendAsync(request, CancellationToken.None);
        try
        {
            var json = await response.Content.ReadAsStringAsync();
            var tree = JsonNode.Parse(json);
            if (tree is null)
                return null;

            var engines = tree["versions"].Deserialize<List<RemoteEngine>>();
            return engines;
        }
        catch (JsonException je)
        {
            Logger.Error(je, "Failed to read flax api.");
            // var box = MessageBoxManager.GetMessageBoxStandard(
            //     "Exception",
            //     $"An exception occured while deserializing information from the Flax API. It's possible the API has changed. Please make an issue at {Globals.RepoUrl}.",
            //     icon: Icon.Error);
            // await box.ShowWindowDialogAsync(App.Current.Desktop.MainWindow!);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<List<GitHubWorkflow>?> GetGithubWorkflows()
    {
        if (preferencesManager.Preferences.GithubAccessToken is null)
            return null;
        var request = new HttpRequestMessage(HttpMethod.Get, GithubWorkflowApiUrl);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("User-Agent", "Seed Launcher for Flax");
        request.Headers.Add("Bearer", preferencesManager.Preferences.GithubAccessToken);

        // _client.DefaultRequestHeaders.Accept.Clear();
        // _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        // _client.DefaultRequestHeaders.Add("User-Agent", "Seed Launcher for Flax");
        // _client.DefaultRequestHeaders.Add("Bearer", prefs.Preferences.GithubAccessToken);
        // _client.Timeout = TimeSpan.FromSeconds(30);

        var list = new List<GitHubWorkflow>();
        try
        {
            var response = await _client.SendAsync(request, CancellationToken.None);
            var json = await response.Content.ReadAsStringAsync();
            var workflows = JsonNode.Parse(json);
            if (workflows is null)
                return null;

            foreach (var workflowNode in workflows["workflow_runs"]!.AsArray())
            {
                var workflow = workflowNode?.Deserialize<GitHubWorkflow>();
                if (workflow is null || !workflow.IsValid)
                    continue;

                workflow.Artifacts = new List<GitHubArtifact>();

                var workflowRequest = new HttpRequestMessage(HttpMethod.Get,
                    $"https://api.github.com/repos/FlaxEngine/FlaxEngine/actions/runs/{workflow.Id}/artifacts");
                workflowRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                workflowRequest.Headers.Add("User-Agent", "Seed Launcher for Flax");
                workflowRequest.Headers.Add("Bearer", preferencesManager.Preferences.GithubAccessToken);

                var workflowResponse = await _client.SendAsync(workflowRequest, CancellationToken.None);
                var artifactsJson = await workflowResponse.Content.ReadAsStringAsync();
                var artifacts = JsonNode.Parse(artifactsJson);
                foreach (var artifactNode in artifacts!["artifacts"]!.AsArray())
                {
                    var artifact = artifactNode.Deserialize<GitHubArtifact>();
                    if (artifact is null)
                        continue;

                    artifact.CommitHash = workflow.CommitHash;

                    var split = artifact.Name.Split('-');
                    var (os, type) = (split[0], split[1]);

                    artifact.IsEditor = string.Equals(type, "Editor", StringComparison.Ordinal)
                                        || string.Equals(type, "EditorDebugSymbols", StringComparison.Ordinal);

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
            // var box = MessageBoxManager.GetMessageBoxStandard(
            //     "Exception",
            //     $"An exception occured while deserializing information from the Github API. Please make an issue at {Globals.RepoUrl}.",
            //     icon: Icon.Error);
            // await box.ShowWindowDialogAsync(App.Current.Desktop.MainWindow!);
            return null;
        }
        catch (HttpRequestException e)
        {
            Logger.Error(e, "Http request exception.");
            // var box = MessageBoxManager.GetMessageBoxStandard(
            //     "Exception",
            //     $"An exception occured while making an http request to Github. Please make an issue at {Globals.RepoUrl}.\nStatus Code: {e.StatusCode}",
            //     icon: Icon.Error);
            // await box.ShowWindowDialogAsync(App.Current.Desktop.MainWindow!);
            return null;
        }

        return list;
    }

    /// <inheritdoc />
    public async Task<Engine> DownloadVersion(RemoteEngine engine, List<RemotePackage> platformTools,
        string installFolderPath)
    {
        DownloadStarted?.Invoke();
        _cancellationTokenSource.TryReset();
        var cancellationToken = _cancellationTokenSource.Token;

        var tempEditorFile = Path.GetTempFileName();
        CurrentAction = $"Downloading {engine.Name}";
        var editorUrl = engine.GetEditorPackage().EditorUrl;
        await using (var file = new FileStream(tempEditorFile, FileMode.Create, FileAccess.Write, FileShare.None))
            await _client.DownloadDataAsync(editorUrl, file, Progress, cancellationToken: cancellationToken);
        // await DownloadFile(editorUrl, tempEditorFile);
        // create sub folder for this engine installation
        var editorInstallFolder = Path.Combine(installFolderPath, engine.Name);

        // TODO: Check for errors
        // ZipFile.ExtractToDirectory(tempEditorFile, editorInstallFolder);
        CurrentAction = "Extracting editor";
        await ZipHelpers.ExtractToDirectoryAsync(tempEditorFile, editorInstallFolder, Progress, cancellationToken);

        var installedPackages = new List<Package>(platformTools.Count);
        foreach (var tools in platformTools)
        {
            CurrentAction = $"Downloading platform tools for {tools.Name}";
            var tmpFile = Path.GetTempFileName();
            await using (var file = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.None))
                await _client.DownloadDataAsync(tools.Url, file, Progress, cancellationToken: cancellationToken);

            var installFolder = Path.Combine(editorInstallFolder, tools.TargetPath);
            CurrentAction = $"Extracting {tools.Name}";
            await ZipHelpers.ExtractToDirectoryAsync(tmpFile, installFolder, Progress, cancellationToken);

            installedPackages.Add(new Package(tools.Name, installFolder));
        }

        CurrentAction = "Done!";

        DownloadFinished?.Invoke();
        return new Engine
        {
            Name = engine.Name,
            Path = editorInstallFolder,
            Version = new NormalVersion(engine.Version),
            InstalledPackages = installedPackages,
            PreferredConfiguration = Engine.Configuration.Release,
            AvailableConfigurations = new List<Engine.Configuration>
            {
                Engine.Configuration.Debug,
                Engine.Configuration.Development,
                Engine.Configuration.Release
            }
        };
    }

    /// <inheritdoc />
    public async Task<Engine> DownloadFromWorkflow(GitHubWorkflow workflow, List<GitHubArtifact> platformTools,
        string installFolderPath)
    {
        DownloadStarted?.Invoke();
        var cancellationToken = _cancellationTokenSource.Token;

        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Add("User-Agent", "Seed Launcher for Flax");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", preferencesManager.Preferences.GithubAccessToken);

        var tempEditorFile = Path.GetTempFileName();
        CurrentAction = $"Downloading commit {workflow.CommitHash}";
        var editorUrl = workflow.EditorArtifact.DownloadUrl;
        await using (var file = new FileStream(tempEditorFile, FileMode.Create, FileAccess.Write, FileShare.None))
            await _client.DownloadDataAsync(editorUrl, file, Progress,
                contentLength: workflow.EditorArtifact.SizeInBytes, cancellationToken: cancellationToken);

        var editorInstallFolder = Path.Combine(installFolderPath, workflow.CommitHash);

        // TODO: Check for errors
        CurrentAction = "Extracting artifact";

        // Because the artifacts are zips inside a zip, we need to extract the nested zip first.
        // To do that, create a folder named after the commit and extract the first zip there.
        // Then extract that zip to the destination folder.
        var nestedZipPath = Path.Combine(Path.GetTempPath(), workflow.CommitHash);
        await ZipHelpers.ExtractToDirectoryAsync(tempEditorFile, nestedZipPath, Progress, cancellationToken);

        CurrentAction = "Extracting editor";
        // TODO: This should return at least one but maybe check that.
        var nestedZip = Directory.GetFiles(nestedZipPath)[0];
        await ZipHelpers.ExtractToDirectoryAsync(nestedZip, editorInstallFolder, Progress, cancellationToken);

        var installedPackages = new List<Package>(platformTools.Count);
        foreach (var tools in platformTools)
        {
            CurrentAction = $"Downloading platform tools for {tools.Name}";
            var tmpFile = Path.GetTempFileName();
            await using (var file = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.None))
                await _client.DownloadDataAsync(tools.DownloadUrl, file, Progress, contentLength: tools.SizeInBytes,
                    cancellationToken: cancellationToken);

            var installFolder = Path.Combine(editorInstallFolder, tools.TargetPath);
            CurrentAction = $"Extracting {tools.Name} artifact";

            var nestedPlatformZipPath = Path.GetRandomFileName();
            await ZipHelpers.ExtractToDirectoryAsync(tmpFile, nestedPlatformZipPath, Progress, cancellationToken);

            CurrentAction = $"Extracting {tools.Name}";
            var nestedPlatformZip = Directory.GetFiles(nestedPlatformZipPath)[0];
            await ZipHelpers.ExtractToDirectoryAsync(nestedPlatformZip, installFolder, Progress, cancellationToken);

            installedPackages.Add(new Package(tools.Name, installFolder));
        }

        CurrentAction = "Done!";

        DownloadFinished?.Invoke();
        return new Engine
        {
            Name = $"CI Build {workflow.CommitHash[..5]}",
            Path = editorInstallFolder,
            Version = new GitVersion(workflow.CommitHash, workflow.CreatedAt),
            InstalledPackages = installedPackages,
            PreferredConfiguration = Engine.Configuration.Release,
            AvailableConfigurations = new List<Engine.Configuration>
            {
                Engine.Configuration.Debug,
                Engine.Configuration.Development,
                Engine.Configuration.Release
            }
        };
    }

    public void StopDownloads()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
    }
}
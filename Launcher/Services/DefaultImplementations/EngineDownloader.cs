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

public class EngineDownloader(IPreferencesManager preferencesManager, IDownloadManager downloadManager)
    : IEngineDownloader
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public const string FlaxLauncherApiUrl = "https://api.flaxengine.com/launcher/engine";

    public const string GithubWorkflowApiUrl =
        "https://api.github.com/repos/FlaxEngine/FlaxEngine/actions/workflows/cd.yml/runs?per_page=1&status=success";

    private readonly HttpClient _client = new();

    /// <inheritdoc />
    public async Task<List<RemoteEngine>?> GetAvailableVersions()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, FlaxLauncherApiUrl);
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("User-Agent", "Seed Launcher for Flax");

        try
        {
            var response = await _client.SendAsync(request, CancellationToken.None);
            try
            {
                var json = await response.Content.ReadAsStringAsync();
                var tree = JsonNode.Parse(json);

                var engines = tree?["versions"].Deserialize(RemoteEngineListGenerationContext.Default.ListRemoteEngine);
                return engines;
            }
            catch (JsonException je)
            {
                Logger.Error(je, "Failed to read flax api.");
                return null;
            }
        }
        catch (HttpRequestException e)
        {
            Logger.Error(e, "Could not get available engine versions due to network issues.");
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
                var workflow = workflowNode?.Deserialize(GitHubWorkflowGenerationContext.Default.GitHubWorkflow);
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
                    var artifact = artifactNode.Deserialize(GitHubArtifactGenerationContext.Default.GitHubArtifact);
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
        var download = new DownloadEntry();
        downloadManager.AddDownload(download);

        var cancellationToken = download.CancellationTokenSource.Token;

        var tempEditorFile = Path.GetTempFileName();
        download.Title = $"Downloading {engine.Name}";
        download.CurrentAction = $"Downloading...";
        var editorUrl = engine.GetEditorPackage().EditorUrl;
        await using (var file = new FileStream(tempEditorFile, FileMode.Create, FileAccess.Write, FileShare.None))
            await _client.DownloadDataAsync(editorUrl, file, download.Progress, cancellationToken: cancellationToken);
        // await DownloadFile(editorUrl, tempEditorFile);
        // create sub folder for this engine installation
        var editorInstallFolder = Path.Combine(installFolderPath, engine.Name);

        // TODO: Check for errors
        // ZipFile.ExtractToDirectory(tempEditorFile, editorInstallFolder);
        download.CurrentAction = "Extracting editor";
        await ZipHelpers.ExtractToDirectoryAsync(tempEditorFile, editorInstallFolder, download.Progress,
            cancellationToken);

        var installedPackages = new List<Package>(platformTools.Count);
        foreach (var tools in platformTools)
        {
            download.CurrentAction = $"Downloading platform tools: {tools.Name}";
            var tmpFile = Path.GetTempFileName();
            await using (var file = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.None))
                await _client.DownloadDataAsync(tools.Url, file, download.Progress,
                    cancellationToken: cancellationToken);

            var installFolder = Path.Combine(editorInstallFolder, tools.TargetPath);
            download.CurrentAction = $"Extracting {tools.Name}";
            await ZipHelpers.ExtractToDirectoryAsync(tmpFile, installFolder, download.Progress, cancellationToken);

            installedPackages.Add(new Package(tools.Name, installFolder));
        }

        download.CurrentAction = "Done!";

        downloadManager.RemoveDownload(download);
        var newEngine = new Engine
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
        newEngine.EnsureMarkedExecutable();
        return newEngine;
    }

    /// <inheritdoc />
    public async Task<Engine> DownloadFromWorkflow(GitHubWorkflow workflow, List<GitHubArtifact> platformTools,
        string installFolderPath)
    {
        var download = new DownloadEntry();
        downloadManager.AddDownload(download);
        var cancellationToken = download.CancellationTokenSource.Token;

        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Add("User-Agent", "Seed Launcher for Flax");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", preferencesManager.Preferences.GithubAccessToken);

        var tempEditorFile = Path.GetTempFileName();
        download.Title = $"Downloading {workflow.CommitHash}";
        download.CurrentAction = $"Downloading...";
        var editorUrl = workflow.EditorArtifact.DownloadUrl;
        await using (var file = new FileStream(tempEditorFile, FileMode.Create, FileAccess.Write, FileShare.None))
            await _client.DownloadDataAsync(editorUrl, file, download.Progress,
                contentLength: workflow.EditorArtifact.SizeInBytes, cancellationToken: cancellationToken);

        var editorInstallFolder = Path.Combine(installFolderPath, workflow.CommitHash);

        // TODO: Check for errors
        download.CurrentAction = "Extracting artifact";

        // Because the artifacts are zips inside a zip, we need to extract the nested zip first.
        // To do that, create a folder named after the commit and extract the first zip there.
        // Then extract that zip to the destination folder.
        var nestedZipPath = Path.Combine(Path.GetTempPath(), workflow.CommitHash);
        await ZipHelpers.ExtractToDirectoryAsync(tempEditorFile, nestedZipPath, download.Progress, cancellationToken);

        download.CurrentAction = "Extracting editor";
        // TODO: This should return at least one but maybe check that.
        var nestedZip = Directory.GetFiles(nestedZipPath)[0];
        await ZipHelpers.ExtractToDirectoryAsync(nestedZip, editorInstallFolder, download.Progress, cancellationToken);

        var installedPackages = new List<Package>(platformTools.Count);
        foreach (var tools in platformTools)
        {
            download.CurrentAction = $"Downloading platform tools: {tools.Name}";
            var tmpFile = Path.GetTempFileName();
            await using (var file = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.None))
                await _client.DownloadDataAsync(tools.DownloadUrl, file, download.Progress,
                    contentLength: tools.SizeInBytes,
                    cancellationToken: cancellationToken);

            var installFolder = Path.Combine(editorInstallFolder, tools.TargetPath);
            download.CurrentAction = $"Extracting {tools.Name} artifact";

            var nestedPlatformZipPath = Path.GetRandomFileName();
            await ZipHelpers.ExtractToDirectoryAsync(tmpFile, nestedPlatformZipPath, download.Progress,
                cancellationToken);

            download.CurrentAction = $"Extracting {tools.Name}";
            var nestedPlatformZip = Directory.GetFiles(nestedPlatformZipPath)[0];
            await ZipHelpers.ExtractToDirectoryAsync(nestedPlatformZip, installFolder, download.Progress,
                cancellationToken);

            installedPackages.Add(new Package(tools.Name, installFolder));
        }

        download.CurrentAction = "Done!";

        downloadManager.RemoveDownload(download);
        var newEngine = new Engine
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
        newEngine.EnsureMarkedExecutable();
        return newEngine;
    }
}
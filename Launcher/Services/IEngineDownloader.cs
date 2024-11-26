using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Launcher.DataModels;

namespace Launcher.Services;

public interface IEngineDownloader
{
    public event Action<string> ActionChanged;
    public event Action DownloadStarted;
    public event Action DownloadFinished;

    public string CurrentAction { get; }
    public Progress<float> Progress { get; }

    public Task<List<RemoteEngine>?> GetAvailableVersions();

    public Task<List<GitHubWorkflow>?> GetGithubWorkflows();

    /// <summary>
    /// Download an editor version along with the specified platform tools.
    /// </summary>
    /// <param name="engine">The engine to download.</param>
    /// <param name="platformTools">The platform tools to also download.</param>
    /// <param name="installFolderPath">The root folder path for all the editor installations. A sub folder for this engine will be created.</param>
    /// <returns></returns>
    public Task<Engine> DownloadVersion(RemoteEngine engine, List<RemotePackage> platformTools,
        string installFolderPath);

    /// <summary>
    /// Download an editor version along with the specified platform tools from a CI run on the FlaxEngine repo.
    /// </summary>
    /// <param name="workflow">The workflow to select the artifacts from.</param>
    /// <param name="platformTools">The platform tools to also download.</param>
    /// <param name="installFolderPath">The root folder path for all the editor installations. A sub folder for this engine will be created.</param>
    /// <returns></returns>
    public Task<Engine> DownloadFromWorkflow(GitHubWorkflow workflow, List<GitHubArtifact> platformTools,
        string installFolderPath);

    public void StopDownloads();
}
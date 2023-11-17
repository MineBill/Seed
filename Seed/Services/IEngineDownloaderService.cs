using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Seed.Models;
using Seed.ViewModels;

namespace Seed.Services;

public interface IEngineDownloaderService
{
    public event Action<string> ActionChanged;
    public string CurrentAction { get; }
    public Progress<float> Progress { get; }

    public Task<List<RemoteEngine>?> GetAvailableVersions();

    public Task<List<Workflow>?> GetGithubWorkflows();

    /// <summary>
    /// Download an editor version along with the specified platform tools.
    /// </summary>
    /// <param name="engine">The engine to download.</param>
    /// <param name="platformTools">The platform tools to also download.</param>
    /// <param name="installFolderPath">The root folder path for all the editor installations. A sub folder for this engine will be created.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<Engine> DownloadVersion(RemoteEngine engine, List<RemotePackage> platformTools,
        string installFolderPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Download an editor version along with the specified platform tools from a CI run on the FlaxEngine repo.
    /// </summary>
    /// <param name="workflow">The workflow to select the artifacts from.</param>
    /// <param name="platformTools">The platform tools to also download.</param>
    /// <param name="installFolderPath">The root folder path for all the editor installations. A sub folder for this engine will be created.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<Engine> DownloadFromWorkflow(Workflow workflow, List<Artifact> platformTools,
        string installFolderPath, CancellationToken cancellationToken = default);
}
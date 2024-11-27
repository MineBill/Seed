using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Launcher.DataModels;

namespace Launcher.Services.Dummies;

public class DummyEngineDownloader : IEngineDownloader
{
    public Task<List<RemoteEngine>?> GetAvailableVersions()
    {
        return Task.FromResult<List<RemoteEngine>?>(null);
    }

    public Task<List<GitHubWorkflow>?> GetGithubWorkflows()
    {
        return Task.FromResult<List<GitHubWorkflow>?>(null);
    }

    public Task<Engine> DownloadVersion(RemoteEngine engine, List<RemotePackage> platformTools,
        string installFolderPath)
    {
        throw new NotImplementedException();
    }

    public Task<Engine> DownloadFromWorkflow(GitHubWorkflow workflow, List<GitHubArtifact> platformTools,
        string installFolderPath)
    {
        throw new NotImplementedException();
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Seed.Models;

namespace Seed.Services;

public interface IEngineDownloaderService
{
    public event Action<string> ActionChanged;
    public string CurrentAction { get; }
    public Progress<float> Progress { get; }
    
    public Task<List<RemoteEngine>?> GetAvailableVersions();

    /// <summary>
    /// Download an editor version along with the specified platform tools.
    /// </summary>
    /// <param name="engine">The engine to download.</param>
    /// <param name="platformTools">The platform tools to also download.</param>
    /// <param name="installFolderPath">The root folder path for all the editor installations. A sub folder for this engine will be created.</param>
    /// <returns></returns>
    public Task<Engine> DownloadVersion(RemoteEngine engine, List<RemotePackage> platformTools, string installFolderPath);
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Seed.Models;

namespace Seed.Services;

public interface IEngineDownloaderService
{
    public Task<List<RemoteEngine>?> GetAvailableVersions();
}
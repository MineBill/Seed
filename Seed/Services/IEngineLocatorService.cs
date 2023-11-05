using System.Collections.Generic;
using Seed.Models;

namespace Seed.Services;

public interface IEngineLocatorService
{
    public List<Engine>? GetInstalledEngines();
}
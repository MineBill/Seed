using System.Collections.Generic;
using Seed.Models;

namespace Seed.Services;

public interface IEngineManager
{
    /// <summary>
    /// Get the list of discovered engines in the installation file.
    /// All the engines are validated and should be available.
    /// </summary>
    /// <returns>The list of installed engines.</returns>
    public List<Engine> GetInstalledEngines();

    /// <summary>
    /// Add a new engine to the database. Will immediately save to the database file.
    /// </summary>
    /// <param name="engine">The new engine to add.</param>
    public void AddEngine(Engine engine);
}
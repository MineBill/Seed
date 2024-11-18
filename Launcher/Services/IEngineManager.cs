using System.Collections.ObjectModel;
using Launcher.DataModels;

namespace Launcher.Services;

public interface IEngineManager
{
    /// <summary>
    /// The list of known projects. Useful to subscribe to updates.
    /// </summary>
    public ObservableCollection<Engine> Engines { get; }

    /// <summary>
    /// Add a new engine to the database. Will immediately save to the database file.
    /// </summary>
    /// <param name="engine">The new engine to add.</param>
    public void AddEngine(Engine engine);

    /// <summary>
    /// Remove an engine from the database. Will handle an invalid passed engine.
    /// Will also immediately save to the database.
    /// </summary>
    /// <param name="engine">The engine to remove.</param>
    public void DeleteEngine(Engine engine);

    /// <summary>
    /// Saves the current list of engines to the file.
    /// Useful when editing the name of an engine.
    /// </summary>
    public void Save();
}
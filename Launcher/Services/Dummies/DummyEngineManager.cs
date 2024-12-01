using System.Collections.ObjectModel;
using Launcher.DataModels;

namespace Launcher.Services.Dummies;

public class DummyEngineManager : IEngineManager
{
    public ObservableCollection<Engine> Engines { get; } = [];

    public void AddEngine(Engine engine)
    {
    }

    public void DeleteEngine(Engine engine)
    {
    }

    public void Save()
    {
    }
}
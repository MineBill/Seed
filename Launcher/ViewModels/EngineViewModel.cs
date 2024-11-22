using System;
using Launcher.DataModels;

namespace Launcher.ViewModels;

public class EngineViewModel(Engine engine) : ViewModelBase
{
    public EngineVersion EngineVersion => engine.Version;

    public string EngineName => engine.Name;

    public EngineViewModel() : this(new Engine()
    {
        Name = "1.9",
        Version = new NormalVersion(Version.Parse("1.9"))
    })
    {
    }
}
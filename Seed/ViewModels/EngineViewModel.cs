using System;
using Seed.Models;

namespace Seed.ViewModels;

public class EngineViewModel: ViewModelBase
{
    private readonly Engine _engine;

    public string Path => _engine.Path;
    public Version Version => _engine.Version;

    public EngineViewModel(Engine engine)
    {
        _engine = engine;
    }

    public EngineViewModel()
    {
        _engine = new Engine
        {
            Path = "/home/minebill/.local/share/Seed/Installs/Flax_1.7",
            Version = new Version(1, 7, 6043, 0)
        };
    }
}
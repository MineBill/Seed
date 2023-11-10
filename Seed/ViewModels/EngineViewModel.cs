using System;
using System.Windows.Input;
using ReactiveUI;
using Seed.Models;
using Seed.Services;

namespace Seed.ViewModels;

public class EngineViewModel : ViewModelBase
{
    private readonly Engine _engine;
    private IEngineManager _engineManager;

    public string Name => _engine.Name;
    public string Path => _engine.Path;
    public Version Version => _engine.Version;

    public ICommand DeleteCommand { get; }

    public EngineViewModel(IEngineManager engineManager, Engine engine)
    {
        _engineManager = engineManager;
        _engine = engine;
        DeleteCommand = ReactiveCommand.Create(() => { _engineManager.DeleteEngine(_engine); });
    }

    public EngineViewModel()
    {
        _engine = new Engine
        {
            Name = "1.7",
            Path = "/home/minebill/.local/share/Seed/Installs/Flax_1.7",
            Version = new Version(1, 7, 6043, 0)
        };
        DeleteCommand = ReactiveCommand.Create(() => { });
    }
}
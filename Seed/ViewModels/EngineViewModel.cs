using System;
using System.Text;
using System.Windows.Input;
using Avalonia.Media;
using ReactiveUI;
using Seed.Models;
using Seed.Services;

namespace Seed.ViewModels;

public class EngineViewModel : ViewModelBase
{
    private readonly Engine _engine;

    public string Name
    {
        get
        {
            if (_engine.Name.Length > 5)
                return _engine.Name[..5];
            return _engine.Name;
        }
    }

    public string Path => _engine.Path;
    public EngineVersion Version => _engine.Version;
    public TextTrimming Trimming => TextTrimming.PrefixCharacterEllipsis;

    public string TrimmedVersion
    {
        get
        {
            if (Version is NormalVersion normal)
                return normal.ToString();
            var build = new StringBuilder(_engine.Version.ToString()?[..8]);
            build.Append("...");
            return build.ToString();
        }
    }

    public ICommand DeleteCommand { get; }

    public EngineViewModel(IEngineManager engineManager, Engine engine)
    {
        _engine = engine;
        DeleteCommand = ReactiveCommand.Create(() => { engineManager.DeleteEngine(_engine); });
    }

    public EngineViewModel()
    {
        _engine = new Engine
        {
            Name = "1.7",
            Path = "/home/minebill/.local/share/Seed/Installs/Flax_1.7",
            Version = new NormalVersion(new Version(1, 7, 6043, 0))
        };
        DeleteCommand = ReactiveCommand.Create(() => { });
    }
}
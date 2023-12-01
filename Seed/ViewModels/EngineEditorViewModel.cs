using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks.Sources;
using System.Windows.Input;
using NLog;
using ReactiveUI;
using Seed.Models;

namespace Seed.ViewModels;

public class EngineEditorViewModel : ViewModelBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly Engine _engine;
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value);
            _engine.Name = value;
        }
    }

    private string _path = string.Empty;

    public string Path
    {
        get => _path;
        set
        {
            this.RaiseAndSetIfChanged(ref _path, value);
            _engine.Path = value;
        }
    }

    private Engine.Configuration _preferredConfiguration;

    public Engine.Configuration PreferredConfiguration
    {
        get => _preferredConfiguration;
        set
        {
            this.RaiseAndSetIfChanged(ref _preferredConfiguration, value);
            _engine.PreferredConfiguration = value;
        }
    }

    public ObservableCollection<Engine.Configuration> AvailableConfigurations { get; set; }

    public ReactiveCommand<string, Unit> OnPathSelected { get; }

    public EngineEditorViewModel(Engine engine)
    {
        _engine = engine;
        Name = engine.Name;
        Path = engine.Path;
        PreferredConfiguration = engine.PreferredConfiguration;
        AvailableConfigurations = new ObservableCollection<Engine.Configuration>(engine.AvailableConfigurations);

        OnPathSelected = ReactiveCommand.Create((string folder) => { });
    }
}
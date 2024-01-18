using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using Seed.Models;
using Seed.Services;

namespace Seed.ViewModels;

public class EngineVersionPickerViewModel: ViewModelBase
{
    private readonly IEngineManager _engineManager;
    private string _title = string.Empty;

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public ObservableCollection<Engine> Engines => _engineManager.Engines;

    private Engine _selectedSelectedEngine;

    public Engine SelectedEngine
    {
        get => _selectedSelectedEngine;
        set => this.RaiseAndSetIfChanged(ref _selectedSelectedEngine, value);
    }

    public ReactiveCommand<Unit, EngineVersion> SaveCommand { get; }

    /// <summary>
    /// Creates a new instance of the EngineVersionPickerViewModel.
    /// </summary>
    /// <param name="engineManager">The engine manager. Ensure that the engine manager has at least one engine installed before passing this.</param>
    /// <param name="project">The project to change the engine for.</param>
    public EngineVersionPickerViewModel(IEngineManager engineManager, Project project)
    {
        _engineManager = engineManager;
        // NOTE: It should be impossible for this viewmodel to be instantiated without any installed engines.

        var found = false;
        foreach (var engine in _engineManager.Engines)
        {
            if (engine.Version == project.EngineVersion)
            {
                _selectedSelectedEngine = engine;
                found = true;
            }
        }
        if (!found)
            _selectedSelectedEngine = _engineManager.Engines[0];
        Title = $"Select Engine for {project.Name}";
        SaveCommand = ReactiveCommand.Create(() => SelectedEngine.Version);
    }
}
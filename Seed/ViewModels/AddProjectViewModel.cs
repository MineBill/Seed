using System;
using System.Collections.ObjectModel;
using System.Reactive;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Seed.Services;

namespace Seed.ViewModels;

public class AddProjectViewModel: ViewModelBase
{
    public ObservableCollection<Version> AvailableEngineVersions { get; } = new();

    private string _projectPath = string.Empty;
    public string ProjectPath
    {
        get => _projectPath;
        set => this.RaiseAndSetIfChanged(ref _projectPath, value);
    }
    
    private Version? _selectedVersion = null;
    public Version? SelectedVersion
    {
        get => _selectedVersion;
        set => this.RaiseAndSetIfChanged(ref _selectedVersion, value);
    }

    // TODO: This feels hacky. Maybe an Interaction<> would be better.
    public ReactiveCommand<Unit, Unit> CloseWindowCommand { get; } = ReactiveCommand.Create(() => { });

    public AddProjectViewModel(string path)
    {
        ProjectPath = path;

        var locator = App.Current.Services.GetService<IEngineLocatorService>();
        if (locator is not null)
        {
            foreach (var engine in locator.GetInstalledEngines()!)
            {
                AvailableEngineVersions.Add(engine.Version);
            }
        }
    }
}
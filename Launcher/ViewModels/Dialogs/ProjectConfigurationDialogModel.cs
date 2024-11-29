using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Launcher.DataModels;

namespace Launcher.ViewModels.Dialogs;

public partial class ProjectConfigurationDialogModel : DialogModelBase<Unit>
{
    [ObservableProperty]
    private Engine? _selectedEngine;

    private readonly Project _project;

    public string ProjectName => _project.Name;

    public List<Engine> Engines { get; }

    /// <inheritdoc/>
    public ProjectConfigurationDialogModel(Project project, List<Engine> engines)
    {
        _project = project;
        Engines = engines;

        SelectedEngine = _project.Engine;
    }

    partial void OnSelectedEngineChanged(Engine? value)
    {
        _project.Engine = value;
        _project.EngineVersion = value?.Version;
    }
}
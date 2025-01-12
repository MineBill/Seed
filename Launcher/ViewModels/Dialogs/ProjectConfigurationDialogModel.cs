using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Launcher.DataModels;

namespace Launcher.ViewModels.Dialogs;

public partial class ProjectConfigurationDialogModel : DialogModelBase<Unit>
{
    [ObservableProperty]
    private Engine? _selectedEngine;

    [ObservableProperty]
    private string _projectArguments;

    [ObservableProperty]
    private bool _isTemplate;

    private readonly Project _project;

    public string ProjectName => _project.Name;

    public List<Engine> Engines { get; }

    /// <inheritdoc/>
    public ProjectConfigurationDialogModel(Project project, List<Engine> engines)
    {
        _project = project;
        Engines = engines;

        SelectedEngine = _project.Engine;
        ProjectArguments = _project.ProjectArguments ?? string.Empty;
        IsTemplate = _project.IsTemplate;
    }

    partial void OnSelectedEngineChanged(Engine? value)
    {
        _project.Engine = value;
        _project.EngineVersion = value?.Version;
    }

    partial void OnProjectArgumentsChanged(string value)
    {
        _project.ProjectArguments = value;
    }

    partial void OnIsTemplateChanged(bool value)
    {
        _project.IsTemplate = value;
    }
}
using System;
using CommunityToolkit.Mvvm.Input;
using Launcher.DataModels;
using Launcher.Services;
using Launcher.Services.Dummies;
using NLog;

namespace Launcher.ViewModels;

public partial class ProjectViewModel(Project project, IProjectManager projectManager) : ViewModelBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public string EngineVersion => project.EngineVersion?.ToString() ?? "null";

    public string ProjectName => project.Name;

    public string IconPath => project.IconPath;

    public ProjectViewModel() : this(
        new Project("Some Project Name", "Path", new NormalVersion(Version.Parse("1.9"))),
        new DummyProjectManager())
    {
    }

    public void ChangeVersion(Version version)
    {
        project.EngineVersion = new NormalVersion(version);
        OnPropertyChanged(nameof(EngineVersion));
    }

    [RelayCommand]
    private void LaunchProject()
    {
        projectManager.RunProject(project);
    }
}
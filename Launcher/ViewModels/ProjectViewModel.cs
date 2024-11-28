using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Launcher.DataModels;
using Launcher.Services;
using Launcher.Services.Dummies;
using Launcher.ViewModels.Dialogs;
using NLog;

namespace Launcher.ViewModels;

public partial class ProjectViewModel : ViewModelBase
{
    private readonly Project _project;
    private readonly IProjectManager _projectManager;
    private readonly IFilesService _filesService;

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public string EngineVersion => _project.EngineVersion?.ToString() ?? "null";

    public string ProjectName => _project.Name;

    public string IconPath => Path.Exists(_project.IconPath)
        ? _project.IconPath
        : "avares://Launcher/Assets/Images/BasicScene.png";

    [ObservableProperty]
    private bool _engineMissing;

    public ProjectViewModel() : this(
        new Project("Some Project Name", "Path", "", new NormalVersion(Version.Parse("1.9"))),
        new DummyProjectManager(),
        new DummyEngineManager(),
        new DummyFileService())
    {
    }

    /// <inheritdoc/>
    public ProjectViewModel(
        Project project,
        IProjectManager projectManager,
        IEngineManager engineManager,
        IFilesService filesService)
    {
        _project = project;
        _projectManager = projectManager;
        _filesService = filesService;

        EngineMissing = _project.Engine is null;
        engineManager.Engines.CollectionChanged += (_, args) =>
        {
            if (args.OldItems != null && args.OldItems.Contains(_project.Engine))
            {
                EngineMissing = true;
            }

            var modified = false;
            // This project view is visible in the projects page and some download finished,
            // adding a new engine installation. We have to figure out if that is compatible
            // with the required version for the project.
            if (args.NewItems != null)
            {
                foreach (var item in args.NewItems)
                {
                    if (item is not Engine engine) continue;
                    if (_project.EngineVersion is NormalVersion projectEngineVersion)
                    {
                        var pVer = projectEngineVersion.Version;
                        if (engine.Version is NormalVersion engineVersion)
                        {
                            var eVer = engineVersion.Version;
                            if (pVer.Major == eVer.Major && pVer.Minor == eVer.Minor && eVer.Revision >= pVer.Revision)
                            {
                                EngineMissing = false;
                                modified = true;
                                _project.Engine = engine;
                                break;
                            }
                        }
                    }
                }
            }

            if (modified)
            {
                _projectManager.Save();
            }
        };
    }

    public void ChangeVersion(Version version)
    {
        _project.EngineVersion = new NormalVersion(version);
        OnPropertyChanged(nameof(EngineVersion));
    }

    [RelayCommand]
    private void OpenConfiguration()
    {
    }

    [RelayCommand]
    private void OpenDirectory()
    {
        _filesService.OpenFolder(_project.Path);
    }

    [RelayCommand]
    private async Task Delete()
    {
        var vm = new ConfirmDialogModel("Are you sure you want to remove this project?");
        var result = await vm.ShowDialog();
        if (result is not null)
        {
            if (result.Result == ConfirmAction.Yes)
            {
                _projectManager.RemoveProject(_project);
            }
        }
    }

    [RelayCommand]
    private void LaunchProject()
    {
        _projectManager.RunProject(_project);
    }
}
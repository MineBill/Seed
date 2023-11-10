using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using ReactiveUI;
using Seed.Models;
using Seed.Services;
using Task = System.Threading.Tasks.Task;

namespace Seed.ViewModels;

public class ProjectViewModel : ViewModelBase
{
    private IProjectManager _projectManager;
    private Project? _project;

    public string? Name => _project?.Name;
    public Version? EngineVersion => _project?.EngineVersion;

    private Bitmap? _icon;

    public Bitmap? Icon
    {
        get => _icon;
        private set => this.RaiseAndSetIfChanged(ref _icon, value);
    }

    private bool _versionInstalled;

    public bool VersionInstalled
    {
        get => _versionInstalled;
        set => this.RaiseAndSetIfChanged(ref _versionInstalled, value);
    }

    public ICommand RemoveProjectCommand { get; }
    public ICommand RunProjectCommand { get; }
    public ICommand OpenProjectFolderCommand { get; }

    public ProjectViewModel(IEngineManager engineManager, IProjectManager projectManager, IFilesService filesService,
        Project project)
    {
        _projectManager = projectManager;
        _project = project;
        RemoveProjectCommand = ReactiveCommand.Create(() =>
        {
            //
            _projectManager.RemoveProject(project);
        });

        RunProjectCommand = ReactiveCommand.Create(() => { _projectManager.RunProject(_project); });

        OpenProjectFolderCommand = ReactiveCommand.Create(() => { filesService.OpenFolder(_project.Path); });
        engineManager.Engines.CollectionChanged += (sender, args) =>
        {
            VersionInstalled =
                engineManager.Engines.Any(x => x.Version == _project.EngineVersion);
        };

        VersionInstalled =
            engineManager.Engines.Any(x => x.Version == _project.EngineVersion);
    }

    public ProjectViewModel()
    {
        _project = new Project("Big AAA Game", "/home/user/dev/projects/Big AAA Game", new Version(1, 6, 6032, 4));
    }

    public async Task LoadIcon()
    {
        if (_project is null || string.IsNullOrWhiteSpace(_project.IconPath))
            return;
        if (!File.Exists(_project.IconPath))
            return;

        Icon = await Task.Run(() => new Bitmap(_project.IconPath));
    }
}
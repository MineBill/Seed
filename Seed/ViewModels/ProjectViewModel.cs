using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MsBox.Avalonia;
using ReactiveUI;
using Seed.Models;
using Seed.Services;
using Task = System.Threading.Tasks.Task;

namespace Seed.ViewModels;

public class ProjectViewModel : ViewModelBase
{
    private readonly IProjectManager _projectManager;

    public Project Project { get; }

    public string Name => Project.Name;
    public EngineVersion? EngineVersion => Project.EngineVersion;
    public string EngineName => Project.Engine?.Name ?? "Missing";

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

    public TextTrimming Trimming => TextTrimming.PrefixCharacterEllipsis;

    private bool _isTemplate;

    public bool IsTemplate
    {
        get => _isTemplate;
        set => this.RaiseAndSetIfChanged(ref _isTemplate, value);
    }

    public bool IsProjectTemplate => Project.IsTemplate;

    public ICommand? RemoveProjectCommand { get; }
    public ICommand? RunProjectCommand { get; }
    public ICommand? MarkAsTemplateCommand { get; }
    public ICommand? ClearCacheCommand { get; }
    public ICommand? OpenProjectFolderCommand { get; }

    public ProjectViewModel(IEngineManager engineManager, IProjectManager projectManager, IFilesService filesService,
        Project project)
    {
        _projectManager = projectManager;
        Project = project;

        engineManager.Engines.CollectionChanged += (_, _) =>
        {
            VersionInstalled =
                engineManager.Engines.Any(x => x.Version == Project.EngineVersion);
        };

        VersionInstalled =
            engineManager.Engines.Any(x => x.Version == Project.EngineVersion);

        RemoveProjectCommand = ReactiveCommand.Create(() => { projectManager.RemoveProject(project); });

        RunProjectCommand = ReactiveCommand.Create(RunProject);

        ClearCacheCommand = ReactiveCommand.Create(ClearCache);

        OpenProjectFolderCommand = ReactiveCommand.Create(() => { filesService.OpenFolder(Project.Path); });

        MarkAsTemplateCommand = ReactiveCommand.Create(() =>
        {
            Project.IsTemplate = !Project.IsTemplate;
            this.RaisePropertyChanged(nameof(IsProjectTemplate));
            _projectManager.Save();
        });
        Task.Run(LoadIcon);
    }

    public ProjectViewModel(Project project, Bitmap? bitmap = null)
    {
        Project = project;
        IsTemplate = true;

        Icon = bitmap;
        if (Icon is null)
            Task.Run(LoadIcon);
    }

    public ProjectViewModel()
    {
        Project = new Project("Big AAA Game", "/home/user/dev/projects/Big AAA Game",
            new NormalVersion(new Version(1, 6, 6032, 4)));
    }

    public async Task LoadIcon()
    {
        var path = Project.IconPath;
        if (!File.Exists(path) || string.IsNullOrWhiteSpace(path))
            return;

        Icon = await Task.Run(() => new Bitmap(path));
    }

    public void OnDoubleTapped(object? sender, TappedEventArgs? e)
    {
        _ = sender;
        _ = e;
        RunProject();
    }

    private void ClearCache()
    {
        if (VersionInstalled)
            _projectManager.ClearCache(Project);
        else
        {
            // TODO: Move this somewhere else so it can be called directly from services.
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Missing Engine",
                """
                The engine this project was last opened in, is missing. Either re-install it, or
                change the associated engine version with this project in the project settings.
                """,
                icon: MsBox.Avalonia.Enums.Icon.Error);
            box.ShowWindowDialogAsync(App.Current.MainWindow);
        }
    }

    private void RunProject()
    {
        if (VersionInstalled)
            _projectManager.RunProject(Project);
        else
        {
            // TODO: Move this somewhere else so it can be called directly from services.
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Missing Engine",
                """
                The engine this project was last opened in, is missing. Either re-install it, or
                change the associated engine version with this project in the project settings.
                """,
                icon: MsBox.Avalonia.Enums.Icon.Error);
            box.ShowWindowDialogAsync(App.Current.MainWindow);
        }
    }
}
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using DynamicData;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NLog;
using ReactiveUI;
using Seed.Models;
using Seed.Services;
using Seed.Services.Dummies;
using Seed.Services.Implementations;

namespace Seed.ViewModels;

public class ProjectsViewModel : ViewModelBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private ProjectViewModel? _selectedProject;
    private CancellationTokenSource? _cancellationTokenSource;

    private readonly IFilesService _filesService;
    private readonly IEngineManager _engineManager;
    private readonly IProjectManager _projectManager;

    public ObservableCollection<ProjectViewModel> Projects { get; private set; } = new();
    public ICommand NewProjectCommand { get; private set; }
    public ICommand AddProjectCommand { get; private set; }

    public Interaction<AddProjectViewModel, Project?> ShowAddProjectDialog { get; } = new();
    public Interaction<NewProjectViewModel, NewProjectDialogResult?> ShowNewProjectDialog { get; } = new();

    public ProjectViewModel? SelectedProject
    {
        get => _selectedProject;
        set => this.RaiseAndSetIfChanged(ref _selectedProject, value);
    }

    public ProjectsViewModel(
        IFilesService filesService,
        IEngineManager engineManager,
        IProjectManager projectManager)
    {
        _filesService = filesService;
        _engineManager = engineManager;
        _projectManager = projectManager;

        NewProjectCommand = ReactiveCommand.Create(NewProject_Clicked);
        AddProjectCommand = ReactiveCommand.Create(AddProject_Clicked);

        _projectManager.Projects.CollectionChanged += (_, args) =>
        {
            if (args.NewItems != null)
                foreach (var project in args.NewItems)
                {
                    Projects.Add(new ProjectViewModel(_engineManager, _projectManager, _filesService,
                        (project as Project)!));
                }

            if (args.OldItems != null)
                foreach (var project in args.OldItems)
                {
                    var old = Projects.Where(x => x.Name == (project as Project)!.Name);
                    Projects.RemoveMany(old);
                }
        };
        LoadProjects();

        // Load icons
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;
        LoadIcons(cancellationToken);
    }

    public ProjectsViewModel()
    {
        _projectManager = new DummyProjectManager();
        _engineManager = new DummyEngineManagerService();
        _filesService = new FilesService(App.Current.MainWindow);

        NewProjectCommand = ReactiveCommand.Create(() => { });
        AddProjectCommand = ReactiveCommand.Create(() => { });
        LoadProjects();
    }

    // Call this after loading all the projects
    private async void LoadIcons(CancellationToken cancellationToken)
    {
        foreach (var project in Projects.ToList())
        {
            await project.LoadIcon();

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private void LoadProjects()
    {
        Projects = new ObservableCollection<ProjectViewModel>();
        foreach (var project in _projectManager.Projects)
        {
            Projects.Add(new ProjectViewModel(_engineManager, _projectManager, _filesService, project));
        }
    }

    private async void NewProject_Clicked()
    {
        if (_engineManager.Engines.Count == 0)
        {
            ShowNoEngineDialog();
            return;
        }
        // Do stuff

        var newProjectViewModel = new NewProjectViewModel(_projectManager, _filesService, _engineManager);

        var result = await ShowNewProjectDialog.Handle(newProjectViewModel);
        if (result is null)
            return;
        Logger.Debug("Creating new project:");
        Logger.Debug($"\tName          = {result.NewProject.Name}");
        Logger.Debug($"\tPath          = {result.NewProject.Path}");
        Logger.Debug($"\tEngineVersion = {result.NewProject.EngineVersion}");

        result.Template.Create(result.NewProject);
    }

    private async void AddProject_Clicked()
    {
        if (_engineManager.Engines.Count == 0)
        {
            ShowNoEngineDialog();
            return;
        }

        var file = await _filesService.SelectFileAsync("Choose a Flax project file", new[]
        {
            new FilePickerFileType("Flax Project")
            {
                Patterns = new[] { "*.flaxproj" }
            }
        });
        if (file is null) return;

        var filePath = file.Path.LocalPath[..^(file.Name.Length+1)];
        var duplicate = _projectManager.Projects.Any(x => filePath.Equals(x.Path));
        if (duplicate)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Error",
                "This project is already added.",
                icon: Icon.Error);
            await box.ShowWindowDialogAsync(App.Current.MainWindow);
            return;
        }

        // TODO: Pass a list of available engine versions over here
        var vm = new AddProjectViewModel(file);

        var result = await ShowAddProjectDialog.Handle(vm);
        if (result is null) return;
        _projectManager.AddProject(result);
        LoadProjects();
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;
        LoadIcons(cancellationToken);
    }

    private static async void ShowNoEngineDialog()
    {
        var box = MessageBoxManager.GetMessageBoxStandard(
            "Warning",
            "No installed engine version detected. Please install an engine version first.",
            icon: Icon.Warning);
        await box.ShowWindowDialogAsync(App.Current.MainWindow);
    }
}
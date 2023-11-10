using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using DynamicData;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using Seed.Models;
using Seed.Services;
using Seed.Services.Dummies;

namespace Seed.ViewModels;

public class ProjectsViewModel : ViewModelBase
{
    private ProjectViewModel? _selectedProject;
    private CancellationTokenSource? _cancellationTokenSource;

    private readonly IFilesService _filesService;
    private readonly IEngineManager _engineManager;
    private readonly IProjectManager _projectManager;

    public ObservableCollection<ProjectViewModel> Projects { get; private set; } = new();
    public ICommand NewProjectCommand { get; private set; }
    public ICommand AddProjectCommand { get; private set; }

    public bool HasAnyProjects => Projects.Count > 0;

    public Interaction<AddProjectViewModel, Project?> ShowAddProjectDialog { get; } = new();

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

        _projectManager.Projects.CollectionChanged += (sender, args) =>
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

    private void NewProject_Clicked()
    {
        if (_engineManager.Engines.Count == 0)
        {
            ShowNoEngineDialog();
            return;
        }
        // Do stuff
    }

    private async void AddProject_Clicked()
    {
        if (_engineManager.Engines.Count == 0)
        {
            ShowNoEngineDialog();
            return;
        }

        var file = await _filesService.OpenFileAsync("Choose a Flax project file", new[]
        {
            new FilePickerFileType("Flax Project")
            {
                Patterns = new[] { "*.flaxproj" }
            }
        });
        if (file is null) return;

        var duplicate = _projectManager.Projects.Any(x => file.Path.ToString().Contains(x.Path));
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
        // var project = new Project
        // (
        //     name,
        //     projectPath
        // );
        //
        // _projectInfo.Projects.Add(project);
        // Projects.Add(new ProjectViewModel(project));
        //
        // ProjectDiscoveryService.Save(_projectInfo);
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using Seed.Models;
using Seed.Services;

namespace Seed.ViewModels;

public class ProjectsViewModel: ViewModelBase
{
    private ProjectViewModel? _selectedProject;
    private CancellationTokenSource? _cancellationTokenSource;
    
    private IFilesService _filesService;
    private IEngineLocatorService _engineLocator;
    private IProjectLocatorService _projectLocator;

    public ObservableCollection<ProjectViewModel> Projects { get; } = new();
    public ICommand NewProjectCommand { get; private set; }
    public ICommand AddProjectCommand { get; private set; }

    public Interaction<AddProjectViewModel, ProjectViewModel?> ShowAddProjectDialog { get; } = new();

    public ProjectViewModel? SelectedProject
    {
        get => _selectedProject;
        set => this.RaiseAndSetIfChanged(ref _selectedProject, value);
    }

    public ProjectsViewModel(
        IFilesService filesService, 
        IEngineLocatorService engineLocator,
        IProjectLocatorService projectLocator)
    {
        _filesService = filesService;
        _engineLocator = engineLocator;
        _projectLocator = projectLocator;
        
        NewProjectCommand = ReactiveCommand.Create(NewProject_Clicked);
        AddProjectCommand = ReactiveCommand.Create(AddProject_Clicked);
        
        foreach (var project in _projectLocator.GetProjects())
        {
            Projects.Add(new ProjectViewModel(project));
        }
        
        // Load icons
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;
        LoadIcons(cancellationToken);
    }

    public ProjectsViewModel()
    {
        Projects = new()
        {
            new(new Project("Pepeges", "/home/minebill/dev/FlaxProjects/Pepeges"))
        };
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

    private void NewProject_Clicked()
    {
        var installed = _engineLocator.GetInstalledEngines();
        if (installed is null || installed.Count == 0)
        {
            ShowNoEngineDialog();
            return;
        }
        // Do stuff
    }
    
    private async void AddProject_Clicked()
    {
        // var installed = _engineLocator.GetInstalledEngines();
        // if (installed is null || installed.Count == 0)
        // {
        //     ShowNoEngineDialog();
        //     return;
        // }
        var file = await _filesService.OpenFileAsync();
        if (file is null) return;

        var projectJson = JsonNode.Parse(await file.OpenReadAsync());
        var name = projectJson?["Name"]?.ToString()!;
        var projectPath = Path.GetDirectoryName(file.TryGetLocalPath()!)!;

        // TODO: Pass a list of available engine versions over here
        var vm = new AddProjectViewModel(projectPath);

        var result = await ShowAddProjectDialog.Handle(vm);
        if (result is not null)
            Projects.Add(result);
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
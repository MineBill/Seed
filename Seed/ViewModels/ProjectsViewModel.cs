using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Seed.Models;
using Seed.Services;

namespace Seed.ViewModels;

public class ProjectsViewModel: ViewModelBase
{
    private static readonly ProjectDiscoveryService ProjectDiscoveryService = new();
    
    private ProjectViewModel? _selectedProject;
    private CancellationTokenSource? _cancellationTokenSource;
    private ProjectInfo _projectInfo;

    public ObservableCollection<ProjectViewModel> Projects { get; } = new();
    public ICommand NewProjectCommand { get; private set; }
    public ICommand AddProjectCommand { get; private set; }

    public ProjectViewModel? SelectedProject
    {
        get => _selectedProject;
        set => this.RaiseAndSetIfChanged(ref _selectedProject, value);
    }

    public ProjectsViewModel()
    {
        NewProjectCommand = ReactiveCommand.Create(NewProject_Clicked);
        AddProjectCommand = ReactiveCommand.Create(AddProject_Clicked);
        
        _projectInfo = ProjectDiscoveryService.Load() ?? new ProjectInfo();
        foreach (var project in _projectInfo.Projects)
        {
            Projects.Add(new ProjectViewModel(project));
        }
        
        // Load icons
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;
        LoadIcons(cancellationToken);
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
        
    }
    
    private async void AddProject_Clicked()
    {
        var filesService = App.Current.Services.GetService<IFilesService>();
        if (filesService == null) return;

        var file = await filesService.OpenFileAsync();
        if (file is null) return;

        var projectJson = JsonNode.Parse(await file.OpenReadAsync());
        var name = projectJson?["Name"]?.ToString()!;
        var projectPath = Path.GetDirectoryName(file.TryGetLocalPath()!)!;
        var project = new Project
        {
            Name = name,
            Version = new EngineVersion(0, 0, 0, 0),
            IconPath = Path.Combine(projectPath, "Cache", "icon.png"),
            Path = projectPath
        };
        
        _projectInfo.Projects.Add(project);
        Projects.Add(new ProjectViewModel(project));
        
        ProjectDiscoveryService.Save(_projectInfo);
    }
}
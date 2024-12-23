using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Launcher.DataModels;
using Launcher.Services;
using Launcher.Services.DefaultImplementations;
using Launcher.Services.Dummies;
using Launcher.ViewModels.Dialogs;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NLog;
using NewProjectDialogModel = Launcher.ViewModels.Dialogs.NewProjectDialogModel;

namespace Launcher.ViewModels;

public partial class ProjectsPageViewModel : PageViewModel
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly IEngineManager _engineManager;
    private readonly IProjectManager _projectManager;
    private readonly IFilesService _filesService;
    private readonly IPreferencesManager _preferencesManager;

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ProjectViewModel> _projects = [];

    [ObservableProperty]
    private IEnumerable<ProjectViewModel> _filteredProjects = [];

    public ProjectsPageViewModel() : this(
        new DummyEngineManager(),
        new DummyProjectManager(),
        new JsonPreferencesManager(),
        new DummyFileService())
    {
    }

    public ProjectsPageViewModel(
        IEngineManager engineManager,
        IProjectManager projectManager,
        IPreferencesManager preferencesManager,
        IFilesService filesService)
    {
        PageName = PageNames.Projects;
        _engineManager = engineManager;
        _projectManager = projectManager;
        _filesService = filesService;
        _preferencesManager = preferencesManager;

        _projectManager.Projects.CollectionChanged += (_, args) =>
        {
            if (args.NewItems != null)
            {
                foreach (var project in args.NewItems)
                {
                    Projects.Add(new ProjectViewModel((project as Project)!, projectManager, _engineManager,
                        _filesService));
                }
            }

            if (args.OldItems != null)
            {
                foreach (var project in args.OldItems)
                {
                    var old = Projects.Where(x => x.ProjectName == (project as Project)!.Name);
                    foreach (var o in old.ToList())
                    {
                        Projects.Remove(o);
                    }
                }
            }
        };
        foreach (var project in _projectManager.Projects)
        {
            Projects.Add(new ProjectViewModel(project, projectManager, _engineManager, _filesService));
        }

        FilteredProjects = Projects;
    }

    [RelayCommand]
    private async Task ShowNewProjectDialog()
    {
        if (_engineManager.Engines.Count == 0)
        {
            await ShowNoEngineDialog();
            return;
        }

        var vm = new NewProjectDialogModel(
            _engineManager,
            _filesService,
            _preferencesManager,
            _projectManager.Projects.Where(p => p.IsTemplate).ToList());
        var result = await vm.ShowDialog();
        if (result is not null)
        {
            if (result.Result is null)
            {
                Logger.Error("Failed to create new project");
                return;
            }

            _projectManager.AddProject(result.Result);
        }
    }

    [RelayCommand]
    private async Task ShowGitCloneDialog()
    {
        if (_engineManager.Engines.Count == 0)
        {
            await ShowNoEngineDialog();
            return;
        }

        var vm = new GitCloneDialogModel(_filesService, _projectManager);
        await vm.ShowDialog();
    }

    [RelayCommand]
    private async Task AddProjectFromDisk()
    {
        if (_engineManager.Engines.Count == 0)
        {
            await ShowNoEngineDialog();
            return;
        }

        var file = await _filesService.SelectFileAsync("Please select existing project file", [
            new FilePickerFileType("Flax Project")
            {
                Patterns = ["*.flaxproj"]
            }
        ]);
        if (file is null) return;

        var filePathWithoutFlaxproj = file.Path.LocalPath[..^(file.Name.Length + 1)];
        var duplicate = _projectManager.Projects.Any(p => p.Path.Equals(filePathWithoutFlaxproj));
        if (duplicate)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Error",
                "This project is already added.",
                icon: Icon.Error);
            await box.ShowWindowDialogAsync(App.Current.Desktop.MainWindow!);
            return;
        }

        var project = _projectManager.ParseProject(file.Path.LocalPath);
        if (project is not null)
        {
            _projectManager.AddProject(project);
        }
    }

    partial void OnSearchTermChanged(string value)
    {
        if (value == string.Empty)
        {
            FilteredProjects = Projects;
            return;
        }

        FilteredProjects = Projects.Where(p =>
            p.ProjectName.Contains(value, StringComparison.InvariantCultureIgnoreCase));
    }

    private static async Task ShowNoEngineDialog()
    {
        var vm = new MessageBoxDialogModel("No engine installation detected. Please install an engine first.",
            MessageDialogActions.Ok);
        await vm.ShowDialog();
    }
}
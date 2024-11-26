using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Launcher.DataModels;
using Launcher.Services;
using Launcher.Services.Dummies;
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

    [ObservableProperty]
    private ObservableCollection<ProjectViewModel> _projects = [];

    public ProjectsPageViewModel() : this(
        new DummyEngineManager(),
        new DummyProjectManager(),
        new DummyFileService())
    {
    }

    public ProjectsPageViewModel(
        IEngineManager engineManager,
        IProjectManager projectManager,
        IFilesService filesService)
    {
        PageName = PageNames.Projects;
        _engineManager = engineManager;
        _projectManager = projectManager;
        _filesService = filesService;

        _projectManager.Projects.CollectionChanged += (_, args) =>
        {
            if (args.NewItems != null)
            {
                foreach (var project in args.NewItems)
                {
                    Projects.Add(new ProjectViewModel((project as Project)!, projectManager));
                }
            }

            if (args.OldItems != null)
            {
                foreach (var project in args.OldItems)
                {
                    var old = Projects.Where(x => x.ProjectName == (project as Project)!.Name);
                    foreach (var o in old)
                    {
                        Projects.Remove(o);
                    }
                }
            }
        };
        foreach (var project in _projectManager.Projects)
        {
            Projects.Add(new ProjectViewModel(project, projectManager));
        }
    }

    [RelayCommand]
    private void AddTestProject()
    {
        Projects.Add(new ProjectViewModel(
            new Project("Project", "Path", new NormalVersion(Version.Parse("1.9"))),
            new DummyProjectManager()));
    }

    [RelayCommand]
    private async Task ShowNewProjectDialog()
    {
        var vm = new NewProjectDialogModel(
            _engineManager,
            _filesService,
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
    private async Task AddProjectFromDisk()
    {
        if (_engineManager.Engines.Count == 0)
        {
            ShowNoEngineDialog();
            return;
        }

        var file = await _filesService.SelectFileAsync("Please select existing project file", [
            new FilePickerFileType("Flax Project")
            {
                Patterns = ["*.flaxproj"]
            }
        ]);
        if (file is null) return;

        var filePathWithoutFlaxproj = file.Path.LocalPath[..^(file.Name.Length+1)];
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

        _projectManager.TryAddProject(file.Path.LocalPath);
    }

    private static async void ShowNoEngineDialog()
    {
        var box = MessageBoxManager.GetMessageBoxStandard(
            "Warning",
            "No engine installation detected. Please install an engine version first.",
            icon: Icon.Warning);
        await box.ShowWindowDialogAsync(App.Current.Desktop.MainWindow!);
    }
}
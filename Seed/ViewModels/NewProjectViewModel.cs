using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Seed.Models;
using Seed.Models.ProjectTemplates;
using Seed.Services;
using Seed.Views;
using Uri = System.Uri;

namespace Seed.ViewModels;

public class NewProjectViewModel : ReactiveValidationObject
{
    private readonly IEngineManager _engineManager;

    // public ProjectContainerViewModel ProjectContainerViewModel { get; }

    private string _projectName = string.Empty;

    [Display(Name = "Name")]
    public string ProjectName
    {
        get => _projectName;
        set => this.RaiseAndSetIfChanged(ref _projectName, value);
    }

    private string _projectPath = string.Empty;

    [Display(Name = "Directory")]
    public string ProjectPath
    {
        get => _projectPath;
        set => this.RaiseAndSetIfChanged(ref _projectPath, value);
    }

    private Engine? _selectedSelectedEngine;

    public Engine? SelectedEngine
    {
        get => _selectedSelectedEngine;
        set => this.RaiseAndSetIfChanged(ref _selectedSelectedEngine, value);
    }

    public ObservableCollection<Engine> Engines => _engineManager.Engines;

    private TemplateViewModel _selectedTemplate;

    public TemplateViewModel SelectedTemplate
    {
        get => _selectedTemplate;
        set => this.RaiseAndSetIfChanged(ref _selectedTemplate, value);
    }

    public ObservableCollection<TemplateViewModel> Templates { get; }

    public ReactiveCommand<Unit, NewProjectDialogResult> CreateProjectCommand { get; }

    // TODO: I don't like this. Maybe there is a better way to communicate with the window.
    public ReactiveCommand<Unit, Unit> CloseWindowCommand { get; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<Unit, Unit> SelectFolderCommand { get; }

    public NewProjectViewModel(IProjectManager projectManager, IFilesService filesService, IEngineManager engineManager)
    {
        _engineManager = engineManager;
        // ProjectContainerViewModel = new ProjectContainerViewModel(projectManager);

        var preferencesSaver = App.Current.Services.GetService<IPreferencesSaver>()!;
        ProjectPath = preferencesSaver.Preferences.NewProjectLocation ?? Globals.GetDefaultProjectLocation();

        CreateProjectCommand =
            ReactiveCommand.Create(() => new NewProjectDialogResult(
                new Project(ProjectName, Path.Combine(ProjectPath, ProjectName), SelectedEngine?.Version),
                SelectedTemplate.ProjectTemplate));
        SelectFolderCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var folder = await filesService.SelectFolderAsync();
            ProjectPath = folder?.Path.AbsolutePath ?? string.Empty;
        });

        SelectedEngine = Engines[0];

        var templates = new List<TemplateViewModel>();
        foreach (var project in projectManager.Projects)
        {
            if (project.IsTemplate)
                templates.Add(new TemplateViewModel(new LocalTemplate(project)));
        }

        // TODO: Maybe don't try and inject this here but abstract it?
        var blankScene = new BuiltinTemplate("Blank Scene", null, SelectedEngine.Version,
            new Bitmap(AssetLoader.Open(new Uri("avares://Seed/Assets/BlankScene.png"))));
        templates.Add(new TemplateViewModel(blankScene));

        var basicScene = new BuiltinTemplate("Basic Scene", new Uri("avares://Seed/Assets/BasicScene.zip"),
            SelectedEngine.Version, new Bitmap(AssetLoader.Open(new Uri("avares://Seed/Assets/BasicScene.png"))));
        templates.Add(new TemplateViewModel(basicScene));
        templates.Sort((a, b) => a.ProjectTemplate.Order - b.ProjectTemplate.Order);

        Templates = new ObservableCollection<TemplateViewModel>(templates);

        _selectedTemplate = Templates[0];
        SetupValidation();
    }

    // TODO: Apply some throttling to the validation somehow?
    private void SetupValidation()
    {
        this.ValidationRule(x => x.ProjectName, name => !string.IsNullOrWhiteSpace(name), "Name cannot be null");
        this.ValidationRule(x => x.ProjectName, name => !name!.Contains(' ') && name.All(char.IsLetterOrDigit),
            "Name cannot contain spaces or symbols");
        this.ValidationRule(x => x.ProjectPath, path =>
        {
            // TODO: Can we compact this?
            if (!string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    var attr = File.GetAttributes(path);
                    if (!attr.HasFlag(FileAttributes.ReadOnly) && attr.HasFlag(FileAttributes.Directory))
                    {
                        return true;
                    }

                    return false;
                }
                catch (Exception e) when (e is DirectoryNotFoundException or FileNotFoundException)
                {
                    return false;
                }
            }

            return false;
        }, "Can't create here");

        var a = this.WhenAnyValue(x => x.ProjectName, x => x.ProjectPath,
                (name, path) => new { Name = name, Path = path })
            .Throttle(TimeSpan.FromMilliseconds(100));

        this.ValidationRule(x => ProjectName, a, state => !Directory.Exists(Path.Combine(state.Path, state.Name)),
            _ => "A folder with that name already exists in the selected folder");

        this.ValidationRule(x => x.SelectedTemplate, project => project is not null,
            "Project cannot be null");

        var t = this.WhenAnyValue(x => x.SelectedEngine, x => x.SelectedTemplate,
                (engine, model) => new { Engine = engine, Template = model })
            .Throttle(TimeSpan.FromMilliseconds(100));
        this.ValidationRule(x => x.SelectedEngine, t, state =>
        {
            if (state.Engine is null)
                return false;
            return state.Engine.Version.CompareTo(state.Template.ProjectTemplate.GetEngineVersion()) == 0;
        }, _ => "Selected template project needs to be upgrade to support this engine.");
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Launcher.DataModels;
using Launcher.Services;
using Launcher.Services.Dummies;
using NLog;

namespace Launcher.ViewModels.Dialogs;

public partial class NewProjectDialogModel : DialogModelBase<Project?>
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private string _name = string.Empty;

    [Required]
    public string Name
    {
        get => _name;
        set
        {
            SetProperty(ref _name, value, true);
            ValidateProperty(ParentFolder, nameof(ParentFolder));
        }
    }

    private string _parentFolder = string.Empty;

    [Required]
    [CustomValidation(typeof(NewProjectDialogModel), nameof(ValidatePath))]
    public string ParentFolder
    {
        get => _parentFolder;
        set => SetProperty(ref _parentFolder, value, true);
    }

    [Required]
    [NotifyDataErrorInfo]
    [ObservableProperty]
    // Opening a NewProjectDialog requires a valid engine installation,
    // so we can assume here that an engine is always installed.
    private Engine _selectedEngine;

    [ObservableProperty]
    private ProjectTemplate _selectedTemplate;

    [ObservableProperty]
    private ObservableCollection<ProjectTemplate> _projectTemplates = [];

    private readonly IEngineManager _engineManager;
    private readonly IFilesService _filesService;

    /// <inheritdoc/>
    public NewProjectDialogModel(
        IEngineManager engineManager,
        IFilesService filesService,
        IPreferencesManager preferencesManager,
        List<Project> templateProjects
    )
    {
        _engineManager = engineManager;
        _filesService = filesService;

        _selectedEngine = engineManager.Engines.First();

        _selectedTemplate = new BundledTemplate(
            "Default",
            "BasicScene",
            "The most basic flax project. Contains the same default script as the official Flax launcher.",
            new Uri("avares://Launcher/Assets/BundledProjects/BasicScene.zip"),
            new Bitmap(AssetLoader.Open(new Uri("avares://Launcher/Assets/BundledProjects/BasicScene.png"))));
        ProjectTemplates.Add(_selectedTemplate);

        foreach (var project in templateProjects)
        {
            ProjectTemplates.Add(new LocalTemplate(project));
        }

        if (preferencesManager.Preferences.NewProjectLocation is not null)
        {
            ParentFolder = preferencesManager.Preferences.NewProjectLocation;
        }

        // Trigger a validation to disable the 'Create' button.
        ValidateAllProperties();
    }

    public NewProjectDialogModel()
    {
        _engineManager = new DummyEngineManager();
        _filesService = new DummyFileService();

        _selectedEngine = new Engine
        {
            Name = "1.9",
            Version = new NormalVersion(Version.Parse("1.9"))
        };

        var template = new BundledTemplate(
            "Default",
            "BasicScene",
            "The most basic flax project. Contains the same default script as the official Flax launcher.",
            new Uri("avares://Launcher/Assets/BundledProjects/BasicScene.zip"),
            new Bitmap(AssetLoader.Open(new Uri("avares://Launcher/Assets/BundledProjects/BasicScene.png"))));
        _selectedTemplate = template;
        ProjectTemplates.Add(template);
    }

    public ObservableCollection<Engine> Engines => _engineManager.Engines;

    public static ValidationResult? ValidatePath(string name, ValidationContext context)
    {
        var instance = (NewProjectDialogModel)context.ObjectInstance;
        if (instance.Name == string.Empty)
            return new ValidationResult("Name is empty");

        if (!Path.Exists(instance.ParentFolder))
            return new ValidationResult($"'{instance.ParentFolder}' is not a valid path");

        if (Path.Exists(Path.Combine(instance.ParentFolder, instance.Name)))
            return new ValidationResult(
                $"The folder '{instance.Name}' already exists inside '{instance.ParentFolder}'");

        return ValidationResult.Success;
    }

    [RelayCommand]
    private async Task SelectParentFolder()
    {
        var folder = await _filesService.SelectFolderAsync();
        if (folder is null)
            return;

        ParentFolder = folder.Path.LocalPath;
    }

    [RelayCommand]
    private async Task CreateProject()
    {
        var project = await SelectedTemplate.Create(Name, ParentFolder, SelectedEngine);
        CloseDialogWithParam(project);
    }
}
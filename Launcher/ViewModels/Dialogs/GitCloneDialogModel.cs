using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Launcher.Services;

namespace Launcher.ViewModels.Dialogs;

public partial class GitCloneDialogModel : DialogModelBase<Unit>
{
    [Required]
    [Url]
    [NotifyDataErrorInfo]
    [ObservableProperty]
    private string _repoURL = string.Empty;

    [ObservableProperty]
    private bool _markAsTemplate;

    [Required]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(GitCloneDialogModel), nameof(ValidateDestination))]
    [ObservableProperty]
    private string _destinationFolder = string.Empty;

    private readonly IFilesService _filesService;
    private readonly IProjectManager _projectManager;

    public GitCloneDialogModel(IFilesService filesService, IProjectManager projectManager)
    {
        _filesService = filesService;
        _projectManager = projectManager;

        ValidateProperty(string.Empty, nameof(RepoURL));
        ValidateProperty(string.Empty, nameof(DestinationFolder));
    }

    [RelayCommand]
    private void CloneRepo()
    {
        _projectManager.AddProjectFromGitRepo(RepoURL, DestinationFolder).ContinueWith(async x =>
        {
            var project = await x;
            if (project is null)
            {
                Console.WriteLine("Project was not valid");
                return;
            }

            project.IsTemplate = MarkAsTemplate;
            _projectManager.AddProject(project);
        });
        CloseDialog();
    }

    [RelayCommand]
    private async Task SelectParentFolder()
    {
        var folder = await _filesService.SelectFolderAsync();
        if (folder is null)
            return;

        DestinationFolder = folder.Path.LocalPath;
    }

    public static ValidationResult? ValidateDestination(string name, ValidationContext context)
    {
        var instance = (GitCloneDialogModel)context.ObjectInstance;
        if (!Path.Exists(instance.DestinationFolder))
            return new ValidationResult($"'{instance.DestinationFolder}' is not a valid path");

        return ValidationResult.Success;
    }
}
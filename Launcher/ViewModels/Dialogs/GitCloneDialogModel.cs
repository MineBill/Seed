using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Launcher.Services;

namespace Launcher.ViewModels.Dialogs;

public partial class GitCloneDialogModel(IFilesService filesService, IProjectManager projectManager)
    : DialogModelBase<Unit>
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
    [ObservableProperty]
    private string _destinationFolder = string.Empty;

    [RelayCommand]
    private void CloneRepo()
    {
        projectManager.AddProjectFromGitRepo(RepoURL, DestinationFolder).ContinueWith(async x =>
        {
            var project = await x;
            if (project is null)
            {
                Console.WriteLine("Project was not valid");
                return;
            }

            project.IsTemplate = MarkAsTemplate;
            projectManager.AddProject(project);
        });
        CloseDialog();
    }

    [RelayCommand]
    private async Task SelectParentFolder()
    {
        var folder = await filesService.SelectFolderAsync();
        if (folder is null)
            return;

        DestinationFolder = folder.Path.LocalPath;
    }
}
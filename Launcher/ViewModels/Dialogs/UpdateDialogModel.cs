using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Launcher.Services;

namespace Launcher.ViewModels.Dialogs;

public partial class UpdateDialogModel(Update update, IFilesService filesService) : DialogModelBase<Unit>
{
    public string PageName => $"Update {update.TagName} available!";
    public string Tag => update.TagName;
    public string Body => update.Body;

    [RelayCommand]
    private void DownloadUpdate()
    {
        filesService.OpenUri(new Uri(update.Url));
    }
}
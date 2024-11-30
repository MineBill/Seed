using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Launcher.Services;

namespace Launcher.ViewModels.Dialogs;

public partial class SettingsDialogModel : DialogModelBase<Unit>
{
    private readonly IFilesService _files;
    private readonly IPreferencesManager _preferences;

    [ObservableProperty]
    private string? _defaultProjectPath;

    public SettingsDialogModel(IPreferencesManager preferences, IFilesService filesService)
    {
        _files = filesService;
        _preferences = preferences;

        DefaultProjectPath = preferences.Preferences.NewProjectLocation;
    }

    [RelayCommand]
    private async Task SelectFolder()
    {
        var folder = await _files.SelectFolderAsync(DefaultProjectPath);
        if (folder is null) return;
        DefaultProjectPath = folder.Path.LocalPath;
    }

    [RelayCommand]
    private void CloseWithoutSaving()
    {
        CloseDialog();
    }

    [RelayCommand]
    private void CloseWithSaving()
    {
        _preferences.Preferences.NewProjectLocation = DefaultProjectPath;

        _preferences.Save();
        CloseDialog();
    }
}
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Launcher.DataModels;
using Launcher.Services;
using Launcher.Services.Dummies;
using Launcher.ViewModels.Dialogs;

namespace Launcher.ViewModels;

public partial class EngineViewModel(Engine engine, IFilesService files, IEngineManager engineManager) : ViewModelBase
{
    public EngineVersion EngineVersion => engine.Version;

    public string EngineName => engine.Name;

    public EngineViewModel() : this(new Engine
        {
            Name = "1.9",
            Version = new NormalVersion(Version.Parse("1.9"))
        },
        new DummyFileService(),
        new DummyEngineManager())
    {
    }

    [RelayCommand]
    private void OpenEngineDirectory()
    {
        files.OpenFolder(engine.Path);
    }

    [RelayCommand]
    private async Task OpenEngineConfiguration()
    {
        var vm = new EngineConfigurationDialogModel(engine, engineManager);
        await vm.ShowDialog();
    }

    [RelayCommand]
    private async Task DeleteEngine()
    {
        var vm = new MessageBoxDialogModel($"Are you sure you want to delete {engine.Name}?",
            MessageDialogActions.No | MessageDialogActions.Yes);
        var result = await vm.ShowDialog();
        if (result is null) return;

        if (result.Result == MessageDialogActions.Yes)
        {
            engineManager.DeleteEngine(engine);
        }
    }
}
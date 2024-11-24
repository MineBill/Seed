using System;
using CommunityToolkit.Mvvm.Input;
using Launcher.DataModels;
using Launcher.Services;
using Launcher.Services.Dummies;

namespace Launcher.ViewModels;

public partial class EngineViewModel(Engine engine, IFilesService files) : ViewModelBase
{
    public EngineVersion EngineVersion => engine.Version;

    public string EngineName => engine.Name;

    public EngineViewModel() : this(new Engine()
    {
        Name = "1.9",
        Version = new NormalVersion(Version.Parse("1.9"))
    }, new DummyFileService())
    {
    }

    [RelayCommand]
    private void OpenEngineDirectory()
    {
        files.OpenFolder(engine.Path);
    }
}
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using Seed.Models;
using Seed.Services;
using Seed.Services.Dummies;

namespace Seed.ViewModels;

public class EnginesViewModel: ViewModelBase
{
    private IEngineManager _engineManager;
    private readonly IEngineDownloaderService _engineDownloader;
    private Engine? _selectedEngine;
    
    public ObservableCollection<EngineViewModel> Engines { get; } = new();
    public ICommand DownloadVersionCommand { get; }
    public Interaction<DownloadVersionsViewModel, DownloadDialogResult?> ShowDownloadVersionDialog { get; } = new();

    public Engine? SelectedEngine
    {
        get => _selectedEngine;
        set => this.RaiseAndSetIfChanged(ref _selectedEngine, value);
    }

    public bool HasAnyEngines => Engines.Count > 0;
    
    public EnginesViewModel(
        IEngineManager engineManager, 
        IEngineDownloaderService engineDownloaderService)
    {
        _engineManager = engineManager;
        _engineDownloader = engineDownloaderService;

        DownloadVersionCommand = ReactiveCommand.CreateFromTask(DownloadVersion_Clicked);
        
        var engines = _engineManager.GetInstalledEngines();
        foreach (var engine in engines)
        {
            Engines.Add(new EngineViewModel(engine));
        }
    }

    public EnginesViewModel()
    {
        _engineManager = new DummyEngineManagerService();
        var engines = _engineManager.GetInstalledEngines();
        foreach (var engine in engines)
        {
            Engines.Add(new EngineViewModel(engine));
        }
    }

    private async Task DownloadVersion_Clicked()
    {
        var versions = await _engineDownloader.GetAvailableVersions();
        if (versions is null)
        {
            // TODO: Log failure to read API
            return;
        }
        
        // TODO: Exclude already installed version from being installed again.

        var downloadViewModel = new DownloadVersionsViewModel(versions);
        
        // TODO: Show popup dialog and give it all the versions
        var version = await ShowDownloadVersionDialog.Handle(downloadViewModel);
        if (version is null)
            return;

        var installLocation = Globals.GetDefaultEngineInstallLocation();
        Console.WriteLine($"Started download of version {version.Engine.Version}");
        Console.WriteLine("Will also download the following platform tools:");
        foreach (var package in version.PlatformTools)
        {
            Console.WriteLine($"\t{package.Name}");
        }
        var engine = await _engineDownloader.DownloadVersion(version.Engine, version.PlatformTools, installLocation);
        _engineManager.AddEngine(engine);
    }
}
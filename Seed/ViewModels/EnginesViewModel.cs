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
    private readonly IEngineLocatorService _engineLocator;
    private readonly IEngineDownloaderService _engineDownloader;
    private Engine? _selectedEngine;
    private EnginesInfo _enginesInfo = new();
    
    public ObservableCollection<EngineViewModel> Engines { get; } = new();
    public ICommand DownloadVersionCommand { get; }
    public Interaction<DownloadVersionsViewModel, DownloadDialogResult?> ShowDownloadVersionDialog { get; } = new();

    public Engine? SelectedEngine
    {
        get => _selectedEngine;
        set => this.RaiseAndSetIfChanged(ref _selectedEngine, value);
    }
    
    public EnginesViewModel(
        IEngineLocatorService engineLocatorService, 
        IEngineDownloaderService engineDownloaderService)
    {
        _engineLocator = engineLocatorService;
        _engineDownloader = engineDownloaderService;

        DownloadVersionCommand = ReactiveCommand.CreateFromTask(DownloadVersion_Clicked);
        
        var engines = _engineLocator.GetInstalledEngines();
        if (engines is null)
        {
            // On windows, this probably means that it's the first launch of the app
            // TODO: Look at the Versions.txt and populate the Engines collection, then save it.
            return;
        }

        _enginesInfo.Engines = engines;

        foreach (var engine in _enginesInfo.Engines)
        {
            Engines.Add(new EngineViewModel(engine));
        }
    }

    public EnginesViewModel()
    {
        _engineLocator = new DummyEngineLocatorService();
        foreach (var engine in _engineLocator.GetInstalledEngines()!)
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

        var downloadViewModel = new DownloadVersionsViewModel(versions);
        
        // TODO: Show popup dialog and give it all the versions
        var version = await ShowDownloadVersionDialog.Handle(downloadViewModel);
        if (version is null)
            return;
        Console.WriteLine($"Download version {version.Engine.Version}");
        Console.WriteLine("Will also download the following platform tools:");
        foreach (var package in version.PlatformTools)
        {
            Console.WriteLine($"\t{package.Name}");
        }
    }
}
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData;
using ReactiveUI;
using Seed.Models;
using Seed.Services;
using Seed.Services.Dummies;

namespace Seed.ViewModels;

public class EnginesViewModel : ViewModelBase
{
    private IEngineManager _engineManager;
    private readonly IEngineDownloaderService _engineDownloader;
    private Engine? _selectedEngine;

    public ObservableCollection<EngineViewModel> Engines { get; private set; } = new();
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

        DownloadVersionCommand = ReactiveCommand.CreateFromTask(OnDownloadVersionButtonClicked);

        // We subscribe se we can update the viewmodel too.
        _engineManager.Engines.CollectionChanged += (sender, args) =>
        {
            if (args.NewItems != null)
                foreach (var engine in args.NewItems)
                {
                    Engines.Add(new EngineViewModel(_engineManager, engine as Engine));
                }

            if (args.OldItems != null)
                foreach (var engine in args.OldItems)
                {
                    var old = Engines.Where(x => x.Version == (engine as Engine).Version);
                    Engines.RemoveMany(old);
                }
        };
        LoadAvailableEngines();
    }

    public EnginesViewModel()
    {
        _engineManager = new DummyEngineManagerService();
        LoadAvailableEngines();
    }

    private async Task OnDownloadVersionButtonClicked()
    {
        var versions = await _engineDownloader.GetAvailableVersions();
        if (versions is null)
        {
            // TODO: Log failure to read API
            return;
        }

        // TODO: Exclude already installed version from being installed again.
        var installedEngines = _engineManager.Engines;
        foreach (var installed in installedEngines)
        {
            foreach (var remote in versions.ToList())
            {
                if (remote.Version == installed.Version)
                    versions.Remove(remote);
            }
        }

        // All versions are installed. Rare scenario.
        if (versions.Count == 0)
            return;

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

    private void LoadAvailableEngines()
    {
        var engines = _engineManager.Engines;
        foreach (var engine in engines)
        {
            Engines.Add(new EngineViewModel(_engineManager, engine));
        }
    }
}
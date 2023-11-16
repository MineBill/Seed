using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NLog;
using ReactiveUI;
using Seed.Models;
using Seed.Services;
using Seed.Services.Dummies;
using Seed.Services.Implementations;

namespace Seed.ViewModels;

public class EnginesViewModel : ViewModelBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly IEngineManager _engineManager;
    private readonly IEngineDownloaderService _engineDownloader;
    private Engine? _selectedEngine;

    public ObservableCollection<StableEngineViewModel> Engines { get; } = new();
    public ICommand? DownloadVersionCommand { get; }
    public ICommand? DownloadWorkflowCommand { get; }

    public Interaction<DownloadVersionsViewModel, DownloadDialogResult<RemoteEngine, RemotePackage>?>
        ShowDownloadVersionDialog { get; } = new();

    public Interaction<DownloadWorkflowsViewModel, DownloadDialogResult<Workflow, Artifact>?>
        ShowDownloadWorkflowDialog { get; } = new();

    public Interaction<DeviceCodeResponse, string?> ShowAuthenticationDialog { get; } = new();

    public Engine? SelectedEngine
    {
        get => _selectedEngine;
        set => this.RaiseAndSetIfChanged(ref _selectedEngine, value);
    }

    public EnginesViewModel(
        IEngineManager engineManager,
        IEngineDownloaderService engineDownloaderService)
    {
        _engineManager = engineManager;
        _engineDownloader = engineDownloaderService;

        DownloadVersionCommand = ReactiveCommand.CreateFromTask(OnDownloadVersionButtonClicked);
        DownloadWorkflowCommand = ReactiveCommand.CreateFromTask(OnDownloadWorkflowsButtonClicked);

        // We subscribe se we can update the viewmodel too.
        _engineManager.Engines.CollectionChanged += (_, args) =>
        {
            if (args.NewItems != null)
                foreach (var engine in args.NewItems)
                {
                    Engines.Add(new StableEngineViewModel(_engineManager, (engine as Engine)!));
                }

            if (args.OldItems != null)
                foreach (var engine in args.OldItems)
                {
                    var old = Engines.Where(x => x.Version == ((engine as Engine)!).Version);
                    Engines.RemoveMany(old);
                }
        };
        LoadAvailableEngines();
    }

    public EnginesViewModel()
    {
        _engineManager = new DummyEngineManagerService();
        _engineDownloader = new LocalEngineDownloaderService();
        LoadAvailableEngines();
    }

    private async Task OnDownloadVersionButtonClicked()
    {
        var versions = await _engineDownloader.GetAvailableVersions();
        if (versions is null)
        {
            // TODO: Log failure to read API
            Logger.Error("Failed to get engine information.");
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

    private async Task OnDownloadWorkflowsButtonClicked()
    {
        var box = MessageBoxManager.GetMessageBoxStandard(
            "Just a reminder",
            """
            Daily CI builds are build from the latest source code changes and might contain some bugs.
            Use them if you want bleeding edge features and/or help test the engine.
            """,
            icon: Icon.Info);
        await box.ShowWindowDialogAsync(App.Current.MainWindow);

        // Check for access token
        var prefs = App.Current.Services.GetService<IPreferencesSaver>()!;
        if (prefs.Preferences.GithubAccessToken is null)
        {
            var authenticator = App.Current.Services.GetService<GithubAuthenticator>()!;
            var response = await authenticator.RequestDeviceCode();
            var token = await ShowAuthenticationDialog.Handle(response);
            if (token is null)
                return;
            prefs.Preferences.GithubAccessToken = token;
            prefs.Save();
        }

        var versions = await _engineDownloader.GetGithubWorkflows();
        if (versions is null)
        {
            Logger.Error("Failed to get ci build information.");

            // TODO: Should also display the reason why: timeouts, bad response, etc.
            box = MessageBoxManager.GetMessageBoxStandard(
                "Github communication failure",
                "Failed to get information about the recent CI builds from Github.",
                icon: Icon.Error);
            await box.ShowWindowDialogAsync(App.Current.MainWindow);
            return;
        }

        var downloadViewModel = new DownloadWorkflowsViewModel(versions);

        // TODO: Show popup dialog and give it all the versions
        var version = await ShowDownloadWorkflowDialog.Handle(downloadViewModel);
        if (version is null)
            return;

        var installLocation = Globals.GetDefaultEngineInstallLocation();
        Console.WriteLine($"Started download of version {version.Engine.CommitHash}");
        Console.WriteLine("Will also download the following platform tools:");
        foreach (var package in version.PlatformTools)
        {
            Console.WriteLine($"\t{package.Name}");
        }

        var engine = await _engineDownloader.DownloadFromWorkflow(version.Engine, version.PlatformTools, installLocation);
        _engineManager.AddEngine(engine);
    }

    private void LoadAvailableEngines()
    {
        var engines = _engineManager.Engines;
        foreach (var engine in engines)
        {
            Engines.Add(new StableEngineViewModel(_engineManager, engine));
        }
    }
}
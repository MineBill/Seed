using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Launcher.DataModels;
using Launcher.Services;
using Launcher.Services.DefaultImplementations;
using Launcher.Services.Dummies;
using Launcher.ViewModels.Dialogs;
using NLog;

namespace Launcher.ViewModels;

public partial class EnginesPageViewModel : PageViewModel
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly IEngineManager _engineManager;
    private readonly IEngineDownloader _engineDownloader;
    private readonly IPreferencesManager _preferencesManager;
    private readonly GithubAuthenticator _githubAuthenticator;
    private readonly IFilesService _filesService;

    [ObservableProperty]
    private ObservableCollection<EngineViewModel> _engines = [];

    public EnginesPageViewModel() : this(
        new DummyEngineManager(),
        new DummyEngineDownloader(),
        new JsonPreferencesManager(),
        new GithubAuthenticator(),
        new DummyFileService())
    {
    }

    public EnginesPageViewModel(
        IEngineManager engineManager,
        IEngineDownloader engineDownloader,
        IPreferencesManager preferencesManager,
        GithubAuthenticator githubAuthenticator,
        IFilesService filesService)
    {
        PageName = PageNames.Installs;
        _engineManager = engineManager;
        _engineDownloader = engineDownloader;
        _preferencesManager = preferencesManager;
        _githubAuthenticator = githubAuthenticator;
        _filesService = filesService;

        _engineManager.Engines.CollectionChanged += (_, args) =>
        {
            if (args.NewItems != null)
            {
                foreach (var engine in args.NewItems)
                {
                    Engines.Add(new EngineViewModel((engine as Engine)!, filesService, engineManager));
                }
            }

            if (args.OldItems != null)
            {
                foreach (var engine in args.OldItems)
                {
                    var old = Engines.Where(x => x.EngineVersion == ((engine as Engine)!).Version);
                    foreach (var o in old.ToList())
                    {
                        Engines.Remove(o);
                    }
                }
            }
        };
        foreach (var engine in _engineManager.Engines)
        {
            Engines.Add(new EngineViewModel(engine, filesService, engineManager));
        }
    }

    [RelayCommand]
    private async Task ShowDownloadDialog()
    {
        var versions = await _engineDownloader.GetAvailableVersions();
        if (versions is null)
        {
            Logger.Error("No engine versions found. Weird.");
            return;
        }

        FilterEngineVersions(versions);

        // All versions are installed. Rare scenario.
        if (versions.Count == 0)
            return;

        var vm = new DownloadEngineDialogModel(versions);
        var result = await vm.ShowDialog();
        if (result is not null)
        {
            Console.WriteLine($"Got request to download {result.Result.Item1.Name}");

            try
            {
                var engine = await _engineDownloader.DownloadVersion(result.Result.Item1, result.Result.Item2,
                    _preferencesManager.GetInstallLocation());
                _engineManager.AddEngine(engine);
            }
            catch (TaskCanceledException tce)
            {
                Logger.Info(tce, "Download was canceled");
            }
        }
    }

    [RelayCommand]
    private async Task ShowGitVersionDownloadDialog()
    {
        var vm = new MessageBoxDialogModel(
            """
            Daily CI builds are built from the latest source code changes and might contain some bugs.
            Use them if you want bleeding edge features and/or help test the engine.
            """,
            MessageDialogActions.Ok);
        await vm.ShowDialog();

        if (_preferencesManager.Preferences.GithubAccessToken is null)
        {
            var response = await _githubAuthenticator.RequestDeviceCode();
            var authDialog = new AuthenticationDialogModel(response, _githubAuthenticator, _filesService);
            var result = await authDialog.ShowDialog();
            if (result is null)
                return;
            Logger.Debug("Got access token: {Token}", result.Result);
            _preferencesManager.Preferences.GithubAccessToken = result.Result;
            _preferencesManager.Save();
        }

        var versions = await _engineDownloader.GetGithubWorkflows();
        if (versions is null)
        {
            var dialog = new MessageBoxDialogModel("No versions found.", MessageDialogActions.Ok);
            await dialog.ShowDialog();
            Logger.Error("No engine versions found. Weird.");
            return;
        }

        FilterEngineVersions(versions);
        if (versions.Count == 0)
        {
            var message = "Already have the latest master version installed.";
            var gitEnginesCount = _engineManager.Engines.Count(e => e.Version is GitVersion);
            if (gitEnginesCount == 0)
            {
                message = "Could not locate a recent CI Build.";
            }

            var dialog = new MessageBoxDialogModel(message, MessageDialogActions.Ok);
            await dialog.ShowDialog();
            return;
        }

        try
        {
            var engine = await _engineDownloader.DownloadFromWorkflow(versions[0], versions[0].SupportedPlatformTools,
                _preferencesManager.GetInstallLocation());
            _engineManager.AddEngine(engine);
        }
        catch (TaskCanceledException tce)
        {
            Logger.Debug(tce, "Task canceled");
        }
        //
        // // versions[0].
        // // FilterEngineVersions(versions);
        //
        // // All versions are installed. Rare scenario.
        // if (versions.Count == 0)
        //     return;
        //
        // var vm = new DownloadEngineDialogModel(versions);
        // var result = await vm.ShowDialog();
        // if (result is not null)
        // {
        //     Console.WriteLine($"Got request to download {result.Result.Item1.Name}");
        //
        //     try
        //     {
        //         var engine = await _engineDownloader.DownloadVersion(result.Result.Item1, result.Result.Item2,
        //             _preferencesManager.GetInstallLocation());
        //         _engineManager.AddEngine(engine);
        //     }
        //     catch (TaskCanceledException tce)
        //     {
        //         Logger.Info(tce, "Download was canceled");
        //     }
        // }
    }

    private void FilterEngineVersions(List<RemoteEngine> versions)
    {
        var totalCount = versions.Count;

        var enginesRemoved = versions.RemoveAll(e => e.SupportedPlatformTools.Count == 0);

        foreach (var installed in _engineManager.Engines)
        {
            foreach (var remote in versions.ToList())
            {
                if (installed.Version is NormalVersion normalVersion && normalVersion.Version == remote.Version)
                    versions.Remove(remote);
            }
        }

        Logger.Info(
            "Found {EnginesCount} available engine versions, {Removed} not supported and {Installed} already installed",
            totalCount, enginesRemoved, _engineManager.Engines.Count);
    }

    private void FilterEngineVersions(List<GitHubWorkflow> versions)
    {
        var totalCount = versions.Count;

        var enginesRemoved = versions.RemoveAll(e => e.SupportedPlatformTools.Count == 0);

        foreach (var installed in _engineManager.Engines)
        {
            foreach (var remote in versions.ToList())
            {
                if (installed.Version is GitVersion gitVersion && gitVersion.Commit == remote.CommitHash)
                    versions.Remove(remote);
            }
        }

        Logger.Info(
            "Found {EnginesCount} available engine versions, {Removed} not supported and {Installed} already installed",
            totalCount, enginesRemoved, _engineManager.Engines.Count);
    }

    private static async Task ShowNoEngineDialog()
    {
        var vm = new MessageBoxDialogModel("No engine installation detected. Please install an engine first.",
            MessageDialogActions.Ok);
        await vm.ShowDialog();
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using Launcher.DataModels;
using Launcher.Services;
using Launcher.Services.DefaultImplementations;
using Launcher.Services.Dummies;
using Launcher.ViewModels.Dialogs;
using NLog;

namespace Launcher.ViewModels;

using EngineDownloadPacket = (RemoteEngine, List<RemotePackage>);

public partial class EnginesPageViewModel : PageViewModel
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly IEngineManager _engineManager;
    private readonly IEngineDownloader _engineDownloader;
    private readonly IPreferencesManager _preferencesManager;

    [ObservableProperty]
    private ObservableCollection<EngineViewModel> _engines = [];

    public EnginesPageViewModel() : this(
        new DummyEngineManager(),
        new DummyEngineDownloader(),
        new JsonPreferencesManager())
    {
    }

    public EnginesPageViewModel(
        IEngineManager engineManager,
        IEngineDownloader engineDownloader,
        IPreferencesManager preferencesManager)
    {
        PageName = PageNames.Installs;
        _engineManager = engineManager;
        _engineDownloader = engineDownloader;
        _preferencesManager = preferencesManager;

        _engineManager.Engines.CollectionChanged += (_, args) =>
        {
            if (args.NewItems != null)
            {
                foreach (var engine in args.NewItems)
                {
                    Engines.Add(new EngineViewModel((engine as Engine)!));
                }
            }

            if (args.OldItems != null)
            {
                foreach (var engine in args.OldItems)
                {
                    var old = Engines.Where(x => x.EngineVersion == ((engine as Engine)!).Version);
                    foreach (var o in old)
                    {
                        Engines.Remove(o);
                    }
                }
            }
        };
        foreach (var engine in _engineManager.Engines)
        {
            Engines.Add(new EngineViewModel(engine));
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

    private async Task DownloadEngine(RemoteEngine engine, List<RemotePackage> packages)
    {
    }
}
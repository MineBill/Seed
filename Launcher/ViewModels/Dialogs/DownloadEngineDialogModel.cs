using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Launcher.DataModels;
using NLog;

namespace Launcher.ViewModels.Dialogs;

using EngineDownloadPacket = (RemoteEngine, List<RemotePackage>);

public partial class DownloadEngineDialogModel(List<RemoteEngine> remoteEngines) : DialogModelBase<EngineDownloadPacket>
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private const string DefaultPageName = "Install Flax";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EngineCount))]
    private ObservableCollection<RemoteEngine> _remoteEngines = new(remoteEngines);

    [ObservableProperty]
    private RemoteEngine? _selectedEngine;

    [ObservableProperty]
    private string _pageName = DefaultPageName;

    [ObservableProperty]
    private ObservableCollection<RemotePackageViewModel>? _selectedRemotePackages;

    public string EngineCount => $"{RemoteEngines.Count} available engines";

    public DownloadEngineDialogModel() : this([
        new RemoteEngine()
        {
            Name = "Engine Name", Version = Version.Parse("1.9"),
            Packages = [new RemotePackage("Blah", "path", "some_url")]
        },
        new RemoteEngine { Name = "Engine Name 2", Version = Version.Parse("1.7") },
        new RemoteEngine { Name = "Engine Name 3", Version = Version.Parse("1.7") },
        new RemoteEngine { Name = "Engine Name 4", Version = Version.Parse("1.7") },
        new RemoteEngine { Name = "Engine Name 5", Version = Version.Parse("1.7") },
        new RemoteEngine { Name = "Engine Name 6", Version = Version.Parse("1.7") }
    ])
    {
    }

    public void SelectEngine(RemoteEngine engine)
    {
        PageName = $"Install Flax {engine.Name}";

        SelectedEngine = engine;
        PopulatePlatformToolsList(SelectedEngine.SupportedPlatformTools);
        // engine.Sup

        // var downloader = App.Current.Services.GetService<IEngineDownloader>()!;
        // downloader.DownloadVersion(SelectedEngine, [], "");
    }

    public void DeselectEngine()
    {
        SelectedEngine = null;
        SelectedRemotePackages = null;
        PageName = DefaultPageName;
    }

    [RelayCommand]
    private async Task InstallSelectedEngine()
    {
        if (SelectedEngine is null || SelectedRemotePackages is null)
        {
            Logger.Error("Tried to install engine but selection was null.");
            return;
        }

        var tools = SelectedRemotePackages.Where(p => p.IsChecked).Select(vm => vm.Package).ToList();

        // try
        // {
        //     var engine = await downloader.DownloadVersion(SelectedEngine, tools, preferences.GetInstallLocation());
        //     engineManager.AddEngine(engine);
        // }
        // catch (TaskCanceledException e)
        // {
        //     Logger.Info(e, "Task canceled");
        // }

        CloseDialogWithParam((SelectedEngine, tools));
    }

    private void PopulatePlatformToolsList(List<RemotePackage> packages)
    {
        SelectedRemotePackages = [];
        var packagesToAdd = packages
            .Where(p => !p.IsEditorPackage)
            .Select(p => new RemotePackageViewModel(p));
        foreach (var package in packagesToAdd)
        {
            if (package.IsCurrentPlatform)
                package.IsChecked = true;
            SelectedRemotePackages.Add(package);
        }
    }
}
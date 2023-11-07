using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using ReactiveUI;
using Seed.Models;
using System;

namespace Seed.ViewModels;

public class DownloadVersionsViewModel: ViewModelBase
{
    private RemoteEngineViewModel _selectedVersion;
    public RemoteEngineViewModel SelectedVersion
    {
        get => _selectedVersion;
        set => this.RaiseAndSetIfChanged(ref _selectedVersion, value);
    }

    public bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public ObservableCollection<RemoteEngineViewModel> AvailableEngines { get; } = new();
    public ReactiveCommand<Unit, DownloadDialogResult?> DownloadCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseWindowCommand { get; }

    public DownloadVersionsViewModel(List<RemoteEngine> engines)
    {
        DownloadCommand = ReactiveCommand.Create<DownloadDialogResult?>(() =>
        {
            var editor = SelectedVersion.Packages.First(x => x.IsEditorPackage);
            var tools = SelectedVersion.Packages.FindAll(x => x.IsChecked);

            return new DownloadDialogResult(SelectedVersion.RemoteEngine, editor.RemotePackage, tools.ConvertAll(x => x.RemotePackage));
        });
        CloseWindowCommand = ReactiveCommand.Create(() => { });
        
        engines.Sort((a, b) => b.CompareTo(a));
        foreach (var engine in engines)
        {
            AvailableEngines.Add(new RemoteEngineViewModel(engine));
        }

        if (AvailableEngines.Count > 0)
            SelectedVersion = AvailableEngines[0];

        this.WhenAnyValue(x => x.SelectedVersion)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(OnSelectedVersionChanged);
    }

    private void OnSelectedVersionChanged(RemoteEngineViewModel viewModel)
    {
        foreach (var packageVm in viewModel.Packages)
        {
            if (packageVm.IsCurrentPlatform)
            {
                packageVm.IsChecked = true;
                break;
            }
        }
    }
}
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Seed.Models;

namespace Seed.ViewModels;

public class DownloadVersionsViewModel: ViewModelBase
{
    private RemoteEngine _selectedVersion;
    public RemoteEngine SelectedVersion
    {
        get => _selectedVersion;
        set => this.RaiseAndSetIfChanged(ref _selectedVersion, value);
    }

    private bool _installWindowsTools;
    public bool InstallWindowsTools
    {
        get => _installWindowsTools;
        set => this.RaiseAndSetIfChanged(ref _installWindowsTools, value);
    }
    
    private bool _installLinuxTools;
    public bool InstallLinuxTools
    {
        get => _installLinuxTools;
        set => this.RaiseAndSetIfChanged(ref _installLinuxTools, value);
    }
    
    private bool _installAndroidTools;
    public bool InstallAndroidTools
    {
        get => _installAndroidTools;
        set => this.RaiseAndSetIfChanged(ref _installAndroidTools, value);
    }

    public bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    
    public ObservableCollection<RemoteEngine> AvailableEngines { get; }
    public ReactiveCommand<Unit, DownloadDialogResult?> DownloadCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseWindowCommand { get; }
        
    public DownloadVersionsViewModel(List<RemoteEngine> engines)
    {
        if (IsWindows)
            _installWindowsTools = true;
        if (IsLinux)
            _installLinuxTools = true;
        
        DownloadCommand = ReactiveCommand.Create<DownloadDialogResult?>(() =>
        {
            var editor = SelectedVersion.Packages.First(x => x.IsEditorPackage);
            var tools = SelectedVersion.Packages.FindAll(x =>
                (InstallLinuxTools && x.IsLinuxTools) || 
                (InstallWindowsTools && x.IsLinuxTools) ||
                (InstallAndroidTools && x.IsAndroidTools));

            return new DownloadDialogResult(SelectedVersion, editor, tools);
        });
        CloseWindowCommand = ReactiveCommand.Create(() => { });
        
        engines.Sort((a, b) => b.CompareTo(a));
        AvailableEngines = new ObservableCollection<RemoteEngine>(engines);
        if (AvailableEngines.Count > 0)
            SelectedVersion = AvailableEngines[0];
    }
}
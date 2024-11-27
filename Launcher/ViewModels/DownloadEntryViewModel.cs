using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Launcher.Services;

namespace Launcher.ViewModels;

public partial class DownloadEntryViewModel : ViewModelBase
{
    [ObservableProperty]
    private float _downloadProgress;

    [ObservableProperty]
    private string _currentActionText = string.Empty;

    public DownloadEntry Entry { get; }

    public DownloadEntryViewModel(DownloadEntry entry)
    {
        Entry = entry;

        CurrentActionText = Entry.CurrentAction;
        Entry.Progress.ProgressChanged += (_, f) => { DownloadProgress = Math.Clamp(f * 100.0f, 0f, 100.0f); };
        Entry.ActionChanged += s => { CurrentActionText = s; };
    }

    public DownloadEntryViewModel()
    {
        Entry = new DownloadEntry()
        {
            Title = "Thing",
            CurrentAction = "Downloading platform tools for platform XYZ"
        };
        CurrentActionText = Entry.CurrentAction;
    }

    [RelayCommand]
    private void CancelDownload()
    {
    }
}
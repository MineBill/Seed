using System;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Launcher.Services;

public partial class DownloadEntry : ObservableObject
{
    public event Action<string>? ActionChanged;
    public event Action? Cancelled;
    public readonly Progress<float> Progress = new();
    public CancellationTokenSource CancellationTokenSource = new();

    [ObservableProperty]
    private string _title = string.Empty;

    private string _currentAction = string.Empty;

    public string CurrentAction
    {
        get => _currentAction;
        set
        {
            _currentAction = value;
            ActionChanged?.Invoke(value);
        }
    }


    public DownloadEntry()
    {
        CancellationTokenSource.TryReset();
    }

    public void Cancel()
    {
        CancellationTokenSource.Cancel();
        CancellationTokenSource = new();
        Cancelled?.Invoke();
    }
}

public interface IDownloadManager
{
    public event Action<DownloadEntry> EntryAdded;
    public event Action<DownloadEntry> EntryRemoved;

    void AddDownload(DownloadEntry entry);
    void RemoveDownload(DownloadEntry entry);
}
using ReactiveUI;
using Seed.Services;

namespace Seed.ViewModels;

public class DownloadInfoViewModel: ViewModelBase
{
    private IEngineDownloaderService _engineDownloader;

    private float _progress;
    public float Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    private string _currentVersion;
    public string CurrentAction
    {
        get => _currentVersion;
        set => this.RaiseAndSetIfChanged(ref _currentVersion, value);
    }

    public DownloadInfoViewModel(IEngineDownloaderService engineDownloader)
    {
        Progress = 0;
        _engineDownloader = engineDownloader;
        _engineDownloader.Progress.ProgressChanged += OnProgressChanged;
        _engineDownloader.ActionChanged += OnActionChanged;
    }

    private void OnProgressChanged(object? sender, float progress)
    {
        Progress = progress;
    }

    private void OnActionChanged(string action)
    {
        CurrentAction = action;
    }
}
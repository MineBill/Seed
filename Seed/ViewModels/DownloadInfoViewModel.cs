using System.Windows.Input;
using ReactiveUI;
using Seed.Services;

namespace Seed.ViewModels;

public class DownloadInfoViewModel : ViewModelBase
{
    private readonly IEngineDownloaderService _engineDownloader;
    private readonly EnginesViewModel _enginesViewModel;

    private float _progress;

    public float Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    private string _currentVersion = string.Empty;

    public string CurrentAction
    {
        get => _currentVersion;
        set => this.RaiseAndSetIfChanged(ref _currentVersion, value);
    }

    public string ProgressFormat
    {
        get
        {
            return $"{CurrentAction} {{1}}%";
        }
    }

    public ICommand CancelActiveAction { get; }

    public DownloadInfoViewModel(IEngineDownloaderService engineDownloader, EnginesViewModel enginesViewModel)
    {
        Progress = 0;
        _engineDownloader = engineDownloader;
        _enginesViewModel = enginesViewModel;
        _engineDownloader.Progress.ProgressChanged += OnProgressChanged;
        _engineDownloader.ActionChanged += OnActionChanged;

        CancelActiveAction = ReactiveCommand.Create(() =>
        {
            CurrentAction = string.Empty;
            Progress = 0;
            _enginesViewModel.CancellationTokenSource.Cancel();
        });
    }

    private void OnProgressChanged(object? sender, float progress)
    {
        Progress = progress;
    }

    private void OnActionChanged(string action)
    {
        CurrentAction = action;
        this.RaisePropertyChanged(nameof(ProgressFormat));
    }
}
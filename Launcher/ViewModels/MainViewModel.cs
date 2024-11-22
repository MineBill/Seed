using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using DialogHostAvalonia.Positioners;
using Launcher.Services;
using Launcher.Services.Dummies;
using Launcher.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public class CenteredDialogPopupPositioner : IDialogPopupPositioner
    {
        /// <inheritdoc />
        public Rect Update(Size anchorRectangle, Size size)
        {
            var x = (anchorRectangle.Width - size.Width) / 2.0;
            var y = (anchorRectangle.Height - size.Height) / 2.0;
            return new Rect(new Point(Math.Round(x), Math.Round(y)), size);
        }
    }

    private readonly ProjectsPageViewModel _projectsPage = new();
    private readonly EnginesPageViewModel _enginesPage = new();

    private IEngineDownloader _engineDownloader;

    [ObservableProperty]
    private bool _sidebarExtended = true;

    [ObservableProperty]
    private string _currentDownloadAction = string.Empty;

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private float _downloadProgress;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProjectsPage))]
    [NotifyPropertyChangedFor(nameof(IsEnginesPage))]
    private ViewModelBase _currentPage;

    public bool IsProjectsPage => CurrentPage == _projectsPage;
    public bool IsEnginesPage => CurrentPage == _enginesPage;

    public CenteredDialogPopupPositioner Positioner { get; } = new();
    public CancellationTokenSource CancellationTokenSource { get; set; } = new();

    public MainViewModel()
    {
        _currentPage = _projectsPage;
        _engineDownloader = new DummyEngineDownloader();
    }

    public MainViewModel(IServiceProvider provider)
    {
        _engineDownloader = provider.GetService<IEngineDownloader>()!;
        _engineDownloader.DownloadStarted += EngineDownloaderOnDownloadStarted;
        _engineDownloader.DownloadFinished += EngineDownloaderOnDownloadFinished;
        _engineDownloader.ActionChanged += EngineDownloaderOnActionChanged;
        _engineDownloader.Progress.ProgressChanged += OnDownloadProgressChanged;

        _projectsPage = new ProjectsPageViewModel(
            provider.GetService<IEngineManager>()!,
            provider.GetService<IProjectManager>()!,
            provider.GetService<IFilesService>()!
        );
        _enginesPage = new EnginesPageViewModel(
            provider.GetService<IEngineManager>()!,
            _engineDownloader,
            provider.GetService<IPreferencesManager>()!,
            CancellationTokenSource.Token
        );
        _currentPage = _projectsPage;
    }

    public void ToggleSidebar()
    {
        SidebarExtended = !SidebarExtended;
    }

    [RelayCommand]
    private void GoToProjects()
    {
        CurrentPage = _projectsPage;
    }

    [RelayCommand]
    private void GoToEngines()
    {
        CurrentPage = _enginesPage;
    }

    [RelayCommand]
    private async Task ShowSettingsDialog()
    {
        var settingsDialog = new SettingsDialogModel();
        var result = await settingsDialog.ShowDialog();
        if (result is not null)
        {
            Console.WriteLine($"Got {result.Result}");
        }
        else
        {
            Console.WriteLine($"Got nothing");
        }
    }

    [RelayCommand]
    private void CancelActiveDownload()
    {
        CancellationTokenSource.Cancel();
        EngineDownloaderOnDownloadFinished();
    }

    private void OnDownloadProgressChanged(object? sender, float e)
    {
        DownloadProgress = Math.Clamp(e * 100.0f, 0f, 100.0f);
    }

    private void EngineDownloaderOnDownloadFinished()
    {
        IsDownloading = false;
    }

    private void EngineDownloaderOnDownloadStarted()
    {
        IsDownloading = true;
        DownloadProgress = 0;
    }

    private void EngineDownloaderOnActionChanged(string action)
    {
        CurrentDownloadAction = action;
    }
}
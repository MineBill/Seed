using System;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia.Positioners;
using Launcher.Services;
using Launcher.ViewModels.Dialogs;

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

    private readonly IEngineDownloader _engineDownloader;
    private readonly Func<PageNames, PageViewModel> _pageFactory;

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
    private PageViewModel _currentPage;

    public bool IsProjectsPage => CurrentPage.PageName == PageNames.Projects;
    public bool IsEnginesPage => CurrentPage.PageName == PageNames.Installs;

    public CenteredDialogPopupPositioner Positioner { get; } = new();

    // public MainViewModel()
    // {
    //     _currentPage = new ProjectsPageViewModel(new DummyEngineManager(), new DummyProjectManager());
    //     _engineDownloader = new DummyEngineDownloader();
    // }

    public MainViewModel(IEngineDownloader engineDownloader, Func<PageNames, PageViewModel> factory)
    {
        _pageFactory = factory;
        _engineDownloader = engineDownloader;
        _engineDownloader.DownloadStarted += EngineDownloaderOnDownloadStarted;
        _engineDownloader.DownloadFinished += EngineDownloaderOnDownloadFinished;
        _engineDownloader.ActionChanged += EngineDownloaderOnActionChanged;
        _engineDownloader.Progress.ProgressChanged += OnDownloadProgressChanged;

        // _projectsPage = new ProjectsPageViewModel(
        //     provider.GetService<IEngineManager>()!,
        //     provider.GetService<IProjectManager>()!,
        //     provider.GetService<IFilesService>()!
        // );
        // _enginesPage = new EnginesPageViewModel(
        //     provider.GetService<IEngineManager>()!,
        //     _engineDownloader,
        //     provider.GetService<IPreferencesManager>()!,
        //     CancellationTokenSource.Token
        // );

        CurrentPage = _pageFactory.Invoke(PageNames.Projects);
    }

    public void ToggleSidebar()
    {
        SidebarExtended = !SidebarExtended;
    }

    [RelayCommand]
    private void GoToProjects()
    {
        CurrentPage = _pageFactory.Invoke(PageNames.Projects);
    }

    [RelayCommand]
    private void GoToEngines()
    {
        CurrentPage = _pageFactory.Invoke(PageNames.Installs);
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
        _engineDownloader.StopDownloads();
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
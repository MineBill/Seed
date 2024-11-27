using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    private readonly IDownloadManager _downloadManager;
    private readonly Func<PageNames, PageViewModel> _pageFactory;

    [ObservableProperty]
    private bool _sidebarExtended = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProjectsPage))]
    [NotifyPropertyChangedFor(nameof(IsEnginesPage))]
    private PageViewModel _currentPage;

    [ObservableProperty]
    private ObservableCollection<DownloadEntryViewModel> _activeDownloads = [];

    public bool IsProjectsPage => CurrentPage.PageName == PageNames.Projects;
    public bool IsEnginesPage => CurrentPage.PageName == PageNames.Installs;

    public CenteredDialogPopupPositioner Positioner { get; } = new();

    // public MainViewModel()
    // {
    //     _currentPage = new ProjectsPageViewModel(new DummyEngineManager(), new DummyProjectManager());
    //     _engineDownloader = new DummyEngineDownloader();
    // }

    public MainViewModel(
        IEngineDownloader engineDownloader,
        IDownloadManager downloadManager,
        Func<PageNames, PageViewModel> factory)
    {
        _pageFactory = factory;
        _engineDownloader = engineDownloader;
        _downloadManager = downloadManager;

        downloadManager.EntryAdded += EngineDownloaderOnDownloadStarted;
        downloadManager.EntryRemoved += EngineDownloaderOnDownloadFinished;

        CurrentPage = _pageFactory.Invoke(PageNames.Projects);
    }

    public MainViewModel()
    {
        ActiveDownloads.Add(new DownloadEntryViewModel(new DownloadEntry() { CurrentAction = "Downloading" }));
        ActiveDownloads.Add(new DownloadEntryViewModel(new DownloadEntry() { CurrentAction = "Downloading" }));
        ActiveDownloads.Add(new DownloadEntryViewModel(new DownloadEntry() { CurrentAction = "Downloading" }));
        ActiveDownloads.Add(new DownloadEntryViewModel(new DownloadEntry() { CurrentAction = "Downloading" }));
        ActiveDownloads.Add(new DownloadEntryViewModel(new DownloadEntry() { CurrentAction = "Downloading" }));
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
    private void AddFakeDownload()
    {
        Task.Run(async () =>
        {
            var download = new DownloadEntry();
            _downloadManager.AddDownload(download);
            IProgress<float> prog = download.Progress;

            download.CurrentAction = "Starting download";
            await Task.Delay(TimeSpan.FromSeconds(1));
            download.CurrentAction = "Progress (1 / 4)";
            prog.Report(0.25f);
            await Task.Delay(TimeSpan.FromSeconds(2));
            download.CurrentAction = "Progress (2 / 4)";
            prog.Report(0.5f);
            await Task.Delay(TimeSpan.FromSeconds(3));
            download.CurrentAction = "Progress (3 / 4)";
            prog.Report(0.75f);
            await Task.Delay(TimeSpan.FromSeconds(1));
            download.CurrentAction = "Progress (4 / 4)";
            prog.Report(1.0f);
            await Task.Delay(TimeSpan.FromSeconds(3));
            _downloadManager.RemoveDownload(download);
        });
    }

    private void EngineDownloaderOnDownloadFinished(DownloadEntry entry)
    {
        ActiveDownloads.Remove(ActiveDownloads.FirstOrDefault(d => d.Entry == entry));
    }

    private void EngineDownloaderOnDownloadStarted(DownloadEntry entry)
    {
        ActiveDownloads.Add(new DownloadEntryViewModel(entry));
    }
}
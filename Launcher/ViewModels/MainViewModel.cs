using System;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using DialogHostAvalonia.Positioners;
using Launcher.Services;
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

    public CenteredDialogPopupPositioner Positioner { get; } = new();

    [ObservableProperty] private bool _sidebarExtended = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProjectsPage))]
    [NotifyPropertyChangedFor(nameof(IsEnginesPage))]
    private ViewModelBase _currentPage;

    public bool IsProjectsPage => CurrentPage == _projectsPage;
    public bool IsEnginesPage => CurrentPage == _enginesPage;

    private readonly ProjectsPageViewModel _projectsPage = new();
    private readonly EnginesPageViewModel _enginesPage = new();

    public MainViewModel()
    {
        _currentPage = _projectsPage;
    }

    public MainViewModel(IServiceProvider provider)
    {
        _projectsPage = new ProjectsPageViewModel(
            provider.GetService<IEngineManager>()!,
            provider.GetService<IProjectManager>()!,
            provider.GetService<IFilesService>()!
        );
        _enginesPage = new EnginesPageViewModel(
            provider.GetService<IEngineManager>()!,
            provider.GetService<IEngineDownloader>()!);
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
}
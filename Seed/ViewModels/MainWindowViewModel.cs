using System;
using Microsoft.Extensions.DependencyInjection;
using Seed.Models;
using Seed.Services;
using Seed.Views;

namespace Seed.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ProjectsViewModel ProjectsViewModel { get; private set; }
    public EnginesViewModel EnginesViewModel { get; private set; }
    public DownloadInfoViewModel DownloadInfoViewModel { get; private set; }
    
    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        var engineLocator = serviceProvider.GetService<IEngineLocatorService>()!;
        var projectLocator = serviceProvider.GetService<IProjectLocatorService>()!;
        var engineDownloader = serviceProvider.GetService<IEngineDownloaderService>()!;
        var engineManager = serviceProvider.GetService<IEngineManager>()!;
        ProjectsViewModel = new ProjectsViewModel(serviceProvider.GetService<IFilesService>()!, engineLocator, projectLocator);
        EnginesViewModel = new EnginesViewModel(engineManager, engineDownloader);
        DownloadInfoViewModel = new DownloadInfoViewModel(engineDownloader);
    }
}

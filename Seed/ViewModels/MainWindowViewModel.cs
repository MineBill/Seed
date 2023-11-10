using System;
using Microsoft.Extensions.DependencyInjection;
using Seed.Services;

namespace Seed.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ProjectsViewModel ProjectsViewModel { get; private set; }
    public EnginesViewModel EnginesViewModel { get; private set; }
    public DownloadInfoViewModel DownloadInfoViewModel { get; private set; }

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        var projectManager = serviceProvider.GetService<IProjectManager>()!;
        var engineDownloader = serviceProvider.GetService<IEngineDownloaderService>()!;
        var engineManager = serviceProvider.GetService<IEngineManager>()!;
        ProjectsViewModel =
            new ProjectsViewModel(serviceProvider.GetService<IFilesService>()!, engineManager, projectManager);
        EnginesViewModel = new EnginesViewModel(engineManager, engineDownloader);
        DownloadInfoViewModel = new DownloadInfoViewModel(engineDownloader);
    }
}
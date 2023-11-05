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
    
    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        var engineLocator = serviceProvider.GetService<IEngineLocatorService>()!;
        var projectLocator = serviceProvider.GetService<IProjectLocatorService>()!;
        ProjectsViewModel = new ProjectsViewModel(serviceProvider.GetService<IFilesService>()!, engineLocator, projectLocator);
        EnginesViewModel = new EnginesViewModel(engineLocator, serviceProvider.GetService<IEngineDownloaderService>()!);
    }
}

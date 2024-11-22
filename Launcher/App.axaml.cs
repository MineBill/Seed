using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Launcher.Services;
using Launcher.Services.DefaultImplementations;
using Launcher.ViewModels;
using Launcher.ViewModels.Dialogs;
using Launcher.ViewModels.Windows;
using Launcher.Views;
using Launcher.Views.Dialogs;
using Launcher.Views.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher;

public class App : Application
{
    public new static App Current => (Application.Current as App)!;
    public IServiceProvider Services { get; private set; } = null!;

    public IClassicDesktopStyleApplicationLifetime Desktop =>
        (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ViewLocator.Register<MainViewModel, MainView>();
        ViewLocator.Register<NewProjectViewModel, NewProjectView>();
        ViewLocator.Register<SettingsDialogModel, SettingsDialog>();
        ViewLocator.Register<ProjectsPageViewModel, ProjectsPageView>();
        ViewLocator.Register<ProjectViewModel, ProjectView>();
        ViewLocator.Register<EnginesPageViewModel, EnginesPageView>();
        ViewLocator.Register<EngineViewModel, EngineView>();
        ViewLocator.Register<DownloadEngineDialogModel, DownloadEngineDialog>();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // We only care about desktop.
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return; // FIXME: Throw an exception here

        var configFolder = Globals.GetConfigFolder();
        if (!Directory.Exists(configFolder))
            Directory.CreateDirectory(configFolder);
        // Register all ViewModel->View relations here to prevent use of reflection in the view locator.

        var mainView = new MainView();
        var services = new ServiceCollection();
        services.AddSingleton<IFilesService>(_ => new FilesService(mainView));
        services.AddSingleton<IEngineDownloader>(_ => new EngineDownloader());
        var engineManager = new EngineManager();
        services.AddSingleton<IEngineManager>(_ => engineManager);
        services.AddSingleton<IProjectManager>(_ => new ProjectManager(engineManager));
        services.AddSingleton<IPreferencesManager>(_ => new JsonPreferencesManager());

        Services = services.BuildServiceProvider();

        mainView.DataContext = new MainViewModel(Services);
        desktop.MainWindow = mainView;

        base.OnFrameworkInitializationCompleted();
    }
}
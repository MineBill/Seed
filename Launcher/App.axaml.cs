using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Syndication;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Launcher.Services;
using Launcher.Services.DefaultImplementations;
using Launcher.ViewModels;
using Launcher.ViewModels.Dialogs;
using Launcher.Views;
using Launcher.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher;

public enum PageNames
{
    None,
    Projects,
    Installs,
    News,
}

public class App : Application
{
    public new static App Current => (Application.Current as App)!;

    public IClassicDesktopStyleApplicationLifetime Desktop =>
        (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        // Register all ViewModel->View relations here to prevent use of reflection in the view locator.
        ViewLocator.Register<MainViewModel, MainView>();
        ViewLocator.Register<NewProjectDialogModel, NewProjectDialog>();
        ViewLocator.Register<SettingsDialogModel, SettingsDialog>();
        ViewLocator.Register<ProjectsPageViewModel, ProjectsPageView>();
        ViewLocator.Register<ProjectViewModel, ProjectView>();
        ViewLocator.Register<EnginesPageViewModel, EnginesPageView>();
        ViewLocator.Register<EngineViewModel, EngineView>();
        ViewLocator.Register<DownloadEngineDialogModel, DownloadEngineDialog>();
        ViewLocator.Register<EngineConfigurationDialogModel, EngineConfigurationDialog>();
        ViewLocator.Register<GitCloneDialogModel, GitCloneDialog>();
        ViewLocator.Register<DownloadEntryViewModel, DownloadEntryView>();
        ViewLocator.Register<MessageBoxDialogModel, MessageBoxDialog>();
        ViewLocator.Register<AuthenticationDialogModel, AuthenticationDialog>();
        ViewLocator.Register<ProjectConfigurationDialogModel, ProjectConfigurationDialog>();
        ViewLocator.Register<NewsPageViewModel, NewsPageView>();
        ViewLocator.Register<UpdateDialogModel, UpdateDialog>();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // We only care about desktop.
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return; // FIXME: Throw an exception here

        var configFolder = Globals.GetConfigFolder();
        if (!Directory.Exists(configFolder))
            Directory.CreateDirectory(configFolder);

        var services = new ServiceCollection();
        services.AddSingleton<Window, MainView>();
        services.AddSingleton<IFilesService, FilesService>();
        services.AddSingleton<IEngineDownloader, EngineDownloader>();
        services.AddSingleton<IEngineManager, EngineManager>();
        services.AddSingleton<IProjectManager, ProjectManager>();
        services.AddSingleton<IPreferencesManager, JsonPreferencesManager>();
        services.AddSingleton<IDownloadManager, DownloadManager>();
        services.AddSingleton<GithubAuthenticator>();
        services.AddSingleton<NewsFetcher>();
        services.AddSingleton<UpdateService>();

        services.AddSingleton<MainViewModel>();

        services.AddTransient<ProjectsPageViewModel>();
        services.AddTransient<EnginesPageViewModel>();
        services.AddTransient<NewsPageViewModel>();
        services.AddTransient<SettingsDialogModel>();
        services.AddTransient<DownloadEngineDialogModel>();

        services.AddSingleton<Func<PageNames, PageViewModel>>(x => type =>
            type switch
            {
                PageNames.Projects => x.GetRequiredService<ProjectsPageViewModel>(),
                PageNames.Installs => x.GetRequiredService<EnginesPageViewModel>(),
                PageNames.News => x.GetRequiredService<NewsPageViewModel>(),
                _ => throw new InvalidOperationException()
            }
        );

        services.AddSingleton<Func<List<NewsItem>>>(x => () => x.GetRequiredService<NewsFetcher>().Items);

        var provider = services.BuildServiceProvider();

        var view = provider.GetRequiredService<Window>();
        view.DataContext = provider.GetRequiredService<MainViewModel>();
        desktop.MainWindow = view;

        base.OnFrameworkInitializationCompleted();
    }
}
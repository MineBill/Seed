using System;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Seed.ViewModels;
using Seed.Views;
using Microsoft.Extensions.DependencyInjection;
using Seed.Services;
using Seed.Services.Implementations;
using Application = Avalonia.Application;
using Window = Avalonia.Controls.Window;

namespace Seed;

public partial class App : Application
{
    public const bool UseLocalDownloader = true;

    public new static App Current => (Application.Current as App)!;
    public IServiceProvider Services { get; private set; }
    public Window MainWindow { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        MainWindow = new MainWindow();
        var services = new ServiceCollection();
        services.AddSingleton<IFilesService>(_ => new FilesService(MainWindow));
        services.AddSingleton<IEngineDownloaderService>(_ =>
            UseLocalDownloader ? new LocalEngineDownloaderService() : new EngineDownloaderService());
        var engineManager = new EngineManager();
        services.AddSingleton<IEngineManager>(_ => engineManager);
        services.AddSingleton<IProjectManager>(_ => new ProjectManager(engineManager));
        services.AddSingleton<IPreferencesSaver>(_ => new JsonPreferencesSaver());
        Services = services.BuildServiceProvider();

        MainWindow.DataContext = new MainWindowViewModel(Services);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
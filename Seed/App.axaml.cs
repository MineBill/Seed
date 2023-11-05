using System;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Seed.ViewModels;
using Seed.Views;
using AvaloniaWebView;
using Gtk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Seed.Services;
using Application = Avalonia.Application;
using Window = Avalonia.Controls.Window;

namespace Seed;

public partial class App : Application
{
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
        services.AddSingleton<IEngineLocatorService>(_ => new EngineLocatorService());
        services.AddSingleton<IEngineDownloaderService>(_ => new EngineDownloaderService());
        services.AddSingleton<IProjectLocatorService>(_ => new ProjectLocatorService());
        Services = services.BuildServiceProvider();

        MainWindow.DataContext = new MainWindowViewModel(Services);
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    public override void RegisterServices()
    {
        base.RegisterServices();
        
        AvaloniaWebViewBuilder.Initialize(default);
    }
}
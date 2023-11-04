using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Seed.ViewModels;
using Seed.Views;
using AvaloniaWebView;
using Microsoft.Extensions.DependencyInjection;
using Seed.Services;

namespace Seed;

public partial class App : Application
{
    public new static App Current => (Application.Current as App)!;
    public IServiceProvider Services { get; private set; }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            
            var services = new ServiceCollection();
            services.AddSingleton<IFilesService>(x => new FilesService(desktop.MainWindow));
            Services = services.BuildServiceProvider();
        }


        base.OnFrameworkInitializationCompleted();
    }

    public override void RegisterServices()
    {
        base.RegisterServices();
        
        AvaloniaWebViewBuilder.Initialize(default);
    }
}
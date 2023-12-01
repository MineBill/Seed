using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;
using NLog;
using ReactiveUI;

namespace Seed;

internal static class Program
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            Globals.GetDefaultEngineInstallLocation();
            // Just making sure the config folder exists,
            // otherwise some operations will fail.
            var configFolder = Globals.GetConfigFolder();
            if (!Directory.Exists(configFolder))
                Directory.CreateDirectory(configFolder);

            RxApp.DefaultExceptionHandler = new ReactiveUIExceptionHandler();
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Caught exception during program lifetime.");
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
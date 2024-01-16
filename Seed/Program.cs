using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;
using Avalonia.Media;
using Avalonia.Svg.Skia;
using NLog;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
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
    // public static AppBuilder WithInterFont(this AppBuilder appBuilder)
    // {
    //     return appBuilder.ConfigureFonts(fontManager =>
    //     {
    //         fontManager.AddFontCollection(new InterFontCollection());
    //     });
    // }
    //
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        GC.KeepAlive(typeof(SvgImageExtension).Assembly);
        GC.KeepAlive(typeof(Avalonia.Svg.Skia.Svg).Assembly);
        IconProvider.Current.Register<FontAwesomeIconProvider>();

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
    }
}
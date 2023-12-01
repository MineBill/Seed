using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Seed.Services;

namespace Seed.Controls;

public class Path : TemplatedControl
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static readonly StyledProperty<string> AbsolutePathProperty = AvaloniaProperty.Register<Path, string>(
        nameof(AbsolutePath));

    public string AbsolutePath
    {
        get => GetValue(AbsolutePathProperty);
        set => SetValue(AbsolutePathProperty, value);
    }

    public static readonly StyledProperty<bool> IsReadOnlyProperty = AvaloniaProperty.Register<Path, bool>(
        nameof(IsReadOnly));

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly StyledProperty<ICommand?> CommandProperty = AvaloniaProperty.Register<Path, ICommand?>(
        nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        const string name = "ChangeButton";
        var button = e.NameScope.Find<Button>(name);
        button!.Click += Button_OnClick;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Command is null)
            return;
        var path = AbsolutePath;
        Task.Run(async () =>
        {
            var fileService = App.Current.Services.GetService<IFilesService>()!;
            var folder = await fileService.SelectFolderAsync(path);
            if (folder is null)
                return;

            Dispatcher.UIThread.InvokeAsync(() => { Command.Execute(folder.Path.AbsolutePath); });
        });
    }
}
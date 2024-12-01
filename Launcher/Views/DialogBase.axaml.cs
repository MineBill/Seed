using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace Launcher.Views;

public class DialogBase : ContentControl
{
    public static readonly StyledProperty<string> PageNameProperty = AvaloniaProperty.Register<DialogBase, string>(
        nameof(PageName));

    public string PageName
    {
        get => GetValue(PageNameProperty);
        set => SetValue(PageNameProperty, value);
    }

    public static readonly StyledProperty<ICommand> CloseCommandProperty =
        AvaloniaProperty.Register<DialogBase, ICommand>(nameof(CloseCommand));

    public ICommand CloseCommand
    {
        get => GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }
}
using Avalonia.Controls;
using Avalonia.Input;
using Launcher.ViewModels;

namespace Launcher.Views;

public partial class NewsPageView : UserControl
{
    public NewsPageView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is NewsPageViewModel vm && sender is Image { Tag: string url })
        {
            vm.OpenInBrowserCommand.Execute(url);
        }
    }
}
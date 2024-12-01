using Avalonia.Controls;
using Avalonia.Input;
using Launcher.ViewModels;

namespace Launcher.Views;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            (DataContext as MainViewModel)?.ToggleSidebar();
        }
    }
}
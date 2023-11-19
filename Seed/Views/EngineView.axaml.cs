using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Seed.ViewModels;

namespace Seed.Views;

public partial class EngineView : UserControl
{
    public EngineView()
    {
        InitializeComponent();
    }

    private void InputElement_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is EngineViewModel viewModel)
        {
            viewModel.OnLostFocus(e);
        }
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is EngineViewModel viewModel)
        {
            viewModel.OnKeyDown(e);
        }
    }
}
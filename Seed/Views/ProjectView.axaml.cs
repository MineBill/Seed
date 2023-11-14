using System;
using Avalonia.Controls;
using Avalonia.Input;
using Seed.ViewModels;

namespace Seed.Views;

public partial class ProjectView : UserControl
{
    public ProjectView()
    {
        InitializeComponent();
    }

    private void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is ProjectViewModel viewModel)
        {
            viewModel.OnDoubleTapped(sender, e);
        }
    }
}
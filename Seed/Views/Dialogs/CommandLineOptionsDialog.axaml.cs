using System;
using Avalonia.Controls;
using Seed.ViewModels;

namespace Seed.Views.Dialogs;

public partial class CommandLineOptionsDialog : Window
{
    public CommandLineOptionsDialog()
    {
        InitializeComponent();
        DataContextProperty.Changed.Subscribe(OnDataContextChanged);
    }

    private void OnDataContextChanged(object _)
    {
        if (DataContext is CommandLineOptionsViewModel viewModel)
        {
            viewModel.SaveCommand.Subscribe(Close);
        }
    }
}
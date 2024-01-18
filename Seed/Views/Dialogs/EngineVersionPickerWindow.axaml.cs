using System;
using Avalonia.Controls;
using Seed.ViewModels;

namespace Seed.Views.Dialogs;

public partial class EngineVersionPickerWindow : Window
{
    public EngineVersionPickerWindow()
    {
        InitializeComponent();
        DataContextProperty.Changed.Subscribe(OnDataContextChanged);
    }

    private void OnDataContextChanged(object _)
    {
        if (DataContext is EngineVersionPickerViewModel viewModel)
        {
            viewModel.SaveCommand.Subscribe(Close);
        }
    }
}
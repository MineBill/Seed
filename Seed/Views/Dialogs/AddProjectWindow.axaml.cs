using System;
using Avalonia.Controls;
using Seed.ViewModels;

namespace Seed.Views.Dialogs;

public partial class AddProjectWindow : Window
{
    public AddProjectWindow()
    {
        InitializeComponent();
        DataContextProperty.Changed.Subscribe(OnDataContextChanged);
    }

    private void OnDataContextChanged(object _)
    {
        if (DataContext is AddProjectViewModel viewModel)
        {
            viewModel.CloseWindowCommand.Subscribe(_ => { Close(); });
            viewModel.AddProjectCommand.Subscribe(Close);
        }
    }
}
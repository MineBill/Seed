using System;
using Avalonia.Controls;
using Seed.ViewModels;

namespace Seed.Views.Dialogs;

public partial class DownloadVersionsWindow : Window
{
    public DownloadVersionsWindow()
    {
        InitializeComponent();
        DataContextProperty.Changed.Subscribe(OnDataContextChanged);
    }

    private void OnDataContextChanged(object _)
    {
        if (DataContext is DownloadVersionsViewModel viewModel)
        {
            viewModel.DownloadCommand.Subscribe(Close);
            viewModel.CloseWindowCommand.Subscribe(_ => Close());
        }
    }
}
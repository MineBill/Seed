using System;
using Avalonia.Controls;
using Seed.ViewModels;

namespace Seed.Views.Dialogs;

public partial class DownloadVersionsWindow : Window
{
    public DownloadVersionsWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is not DownloadVersionsViewModel viewModel) return;
            viewModel.DownloadCommand.Subscribe(Close);
            viewModel.CloseWindowCommand.Subscribe(_ => Close());
        };
    }
}
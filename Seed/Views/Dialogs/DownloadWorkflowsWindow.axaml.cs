using System;
using Avalonia.Controls;
using Seed.ViewModels;

namespace Seed.Views.Dialogs;

public partial class DownloadWorkflowsWindow : Window
{
    public DownloadWorkflowsWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is not DownloadWorkflowsViewModel viewModel) return;
            viewModel.DownloadCommand.Subscribe(Close);
            viewModel.CloseWindowCommand.Subscribe(_ => Close());
        };
    }
}
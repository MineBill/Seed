using System;
using Avalonia.Controls;
using Seed.ViewModels;

namespace Seed.Views.Dialogs;

public partial class AuthenticationDialog : Window
{
    public AuthenticationDialog()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is AuthenticationDialogViewModel viewModel)
            {
                viewModel.AuthProcessFinishedEvent += (sender, s) => { Close(s); };
            }
        };
        Closing += OnClosing;
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is AuthenticationDialogViewModel viewModel)
        {
            // TODO: Cancel the token.
            viewModel.CancellationTokenSource.Cancel();
        }
    }
}
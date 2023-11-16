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
    }
}
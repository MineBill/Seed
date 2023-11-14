using System;
using Avalonia.Controls;
using Seed.ViewModels;

namespace Seed.Views.Dialogs;

public partial class NewProjectWindow : Window
{
    public NewProjectWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is NewProjectViewModel viewModel)
            {
                viewModel.CloseWindowCommand.Subscribe(_ => { Close(); });
                viewModel.CreateProjectCommand.Subscribe(Close);
            }
        };
    }
}
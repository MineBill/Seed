using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using Seed.Models;
using Seed.ViewModels;
using Seed.Views.Dialogs;

namespace Seed.Views;

public partial class ProjectsView : UserControl
{
    public ProjectsView()
    {
        InitializeComponent();
        DataContextProperty.Changed.Subscribe(OnDataContextChanged);
    }

    private void OnDataContextChanged(object _)
    {
        if (DataContext is ProjectsViewModel viewModel)
        {
            viewModel.ShowAddProjectDialog.RegisterHandler(DoShowDialogAsync);
            viewModel.ShowNewProjectDialog.RegisterHandler(DoShowNewProjectDialogAsync);
        }
    }

    private static async Task DoShowDialogAsync(InteractionContext<AddProjectViewModel, Project?> interaction)
    {
        var dialog = new AddProjectWindow
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<Project?>(App.Current.MainWindow);
        interaction.SetOutput(result);
    }

    private static async Task DoShowNewProjectDialogAsync(InteractionContext<NewProjectViewModel, NewProjectDialogResult?> interaction)
    {
        var dialog = new NewProjectWindow
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<NewProjectDialogResult?>(App.Current.MainWindow);
        interaction.SetOutput(result);
    }
}
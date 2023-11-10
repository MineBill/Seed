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
        }
    }

    private async Task DoShowDialogAsync(InteractionContext<AddProjectViewModel, Project?> interaction)
    {
        var dialog = new AddProjectWindow
        {
            DataContext = interaction.Input
        };

        // TODO: AAAAAAAAAA WHY NO WINDOW??
        var result = await dialog.ShowDialog<Project?>(App.Current.MainWindow);
        Console.WriteLine($"Is: {result}");
        interaction.SetOutput(result);
    }
}
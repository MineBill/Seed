using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using ReactiveUI;
using Seed.Models;
using Seed.Services;
using Seed.ViewModels;
using Seed.Views.Dialogs;

namespace Seed.Views;

public partial class ProjectsView : UserControl
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

    private void SortingOption_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var vm = DataContext as ProjectsViewModel;
        var comboBox = sender as ComboBox;
        var prefs = App.Current.Services.GetService<IPreferencesSaver>()!;

        var index = comboBox!.SelectedIndex;
        if (index == -1)
        {
            comboBox!.SelectedIndex = (int)prefs.Preferences.ProjectSortingType;
        }
        else
        {
            var type = (SortingType)index;
            prefs.Preferences.ProjectSortingType = type;
            vm?.RefreshProjects();
        }
    }
}
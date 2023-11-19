using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using Seed.Models;
using Seed.Services.Implementations;
using Seed.ViewModels;
using Seed.Views.Dialogs;

namespace Seed.Views;

public partial class EnginesView : UserControl
{
    public EnginesView()
    {
        InitializeComponent();
        DataContextProperty.Changed.Subscribe(OnDataContextChanged);
    }

    private void OnDataContextChanged(object _)
    {
        if (DataContext is EnginesViewModel viewModel)
        {
            viewModel.ShowDownloadVersionDialog.RegisterHandler(ShowVersionsDialogAsync);
            viewModel.ShowDownloadWorkflowDialog.RegisterHandler(ShowWorkflowsDialogAsync);
            viewModel.ShowAuthenticationDialog.RegisterHandler(ShowAuthenticationDialogAsync);
        }
    }

    private static async Task ShowAuthenticationDialogAsync(InteractionContext<DeviceCodeResponse, string?> context)
    {
        var dialog = new AuthenticationDialog
        {
            DataContext = new AuthenticationDialogViewModel(context.Input)
        };
        context.SetOutput(await dialog.ShowDialog<string?>(App.Current.MainWindow));
    }

    private static async Task ShowVersionsDialogAsync(
        InteractionContext<DownloadVersionsViewModel, DownloadDialogResult<RemoteEngine, RemotePackage>?> interaction)
    {
        var dialog = new DownloadVersionsWindow
        {
            DataContext = interaction.Input,
        };

        interaction.SetOutput(
            await dialog.ShowDialog<DownloadDialogResult<RemoteEngine, RemotePackage>?>(App.Current.MainWindow));
    }

    private static async Task ShowWorkflowsDialogAsync(
        InteractionContext<DownloadWorkflowsViewModel, DownloadDialogResult<Workflow, Artifact>?> interaction)
    {
        var dialog = new DownloadWorkflowsWindow
        {
            DataContext = interaction.Input,
        };

        interaction.SetOutput(
            await dialog.ShowDialog<DownloadDialogResult<Workflow, Artifact>?>(App.Current.MainWindow));
    }
}
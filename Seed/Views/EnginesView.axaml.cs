using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using Seed.Models;
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
            viewModel.ShowDownloadVersionDialog.RegisterHandler(DoShowDialogAsync);
        }
    }

    private async Task DoShowDialogAsync(InteractionContext<DownloadVersionsViewModel, DownloadDialogResult?> interaction)
    {
        var dialog = new DownloadVersionsWindow
        {
            DataContext = interaction.Input,
        };

        interaction.SetOutput(await dialog.ShowDialog<DownloadDialogResult?>(App.Current.MainWindow));
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Launcher.ViewModels.Dialogs;

namespace Launcher.Views.Dialogs;

public partial class EngineConfigurationDialog : UserControl
{
    public EngineConfigurationDialog()
    {
        InitializeComponent();
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        (DataContext as EngineConfigurationDialogModel)?.SelectionChangedCommand.Execute(e.AddedItems);
    }
}
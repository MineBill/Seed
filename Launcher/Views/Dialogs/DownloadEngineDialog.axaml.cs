using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using Launcher.DataModels;
using Launcher.ViewModels.Dialogs;

namespace Launcher.Views.Dialogs;

public partial class DownloadEngineDialog : UserControl
{
    public DownloadEngineDialog()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Pages.Next();
    }

    [RelayCommand]
    private void SelectEngine(RemoteEngine engine)
    {
        Pages.Next();
        (DataContext as DownloadEngineDialogModel)?.SelectEngine(engine);
    }

    [RelayCommand]
    private void GoBack()
    {
        Pages.Previous();
        (DataContext as DownloadEngineDialogModel)?.DeselectEngine();
    }
}
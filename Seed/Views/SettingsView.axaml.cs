using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Seed.ViewModels;

namespace Seed.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void ColorView_OnColorChanged(object? sender, ColorChangedEventArgs e)
    {
        if (DataContext is SettingsViewModel viewModel)
        {
            viewModel.SetAccentColor(e.NewColor);
        }
    }
}
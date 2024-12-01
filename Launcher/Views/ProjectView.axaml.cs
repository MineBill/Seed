using Avalonia.Controls;
using Avalonia.Input;
using Launcher.ViewModels;

namespace Launcher.Views;

public partial class ProjectView : UserControl
{
    public ProjectView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.ClickCount != 2)
            return;

        (DataContext as ProjectViewModel)?.LaunchProjectCommand.Execute(null);
    }
}
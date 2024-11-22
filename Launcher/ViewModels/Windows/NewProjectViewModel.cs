using CommunityToolkit.Mvvm.ComponentModel;
using Launcher.ViewModels.Dialogs;

namespace Launcher.ViewModels.Windows;

public partial class NewProjectViewModel : DialogModelBase<Unit>
{
    [ObservableProperty] private string _thingy = "Text";
}
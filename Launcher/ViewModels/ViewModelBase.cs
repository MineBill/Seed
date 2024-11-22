using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Launcher.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isDebugging = Debugger.IsAttached;
}
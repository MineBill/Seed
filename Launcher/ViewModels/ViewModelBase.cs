using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Launcher.ViewModels;

public partial class ViewModelBase : ObservableValidator
{
    [ObservableProperty]
    private bool _isDebugging = Debugger.IsAttached;
}
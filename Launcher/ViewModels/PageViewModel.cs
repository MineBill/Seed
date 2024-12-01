using CommunityToolkit.Mvvm.ComponentModel;

namespace Launcher.ViewModels;

public partial class PageViewModel : ViewModelBase
{
    [ObservableProperty]
    private PageNames _pageName;
}
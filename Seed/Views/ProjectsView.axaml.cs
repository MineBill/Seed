using Avalonia.Controls;
using Seed.ViewModels;

namespace Seed.Views;

public partial class ProjectsView : UserControl
{
    public ProjectsView()
    {
        InitializeComponent();
        DataContext = new ProjectsViewModel();
    }
}
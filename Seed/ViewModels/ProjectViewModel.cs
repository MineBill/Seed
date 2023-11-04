using System.IO;
using Avalonia.Media.Imaging;
using GLib;
using ReactiveUI;
using Seed.Models;
using Task = System.Threading.Tasks.Task;

namespace Seed.ViewModels;

public class ProjectViewModel: ViewModelBase
{
    private Project _project = new() { Name = "Project Title", Version = new EngineVersion(2, 3, 0000, 1) };
    private Bitmap _icon;
    
    public string Name => _project.Name;
    
    public EngineVersion EngineVersion => _project.Version;

    public Bitmap Icon
    {
        get => _icon;
        private set => this.RaiseAndSetIfChanged(ref _icon, value);
    }

    public ProjectViewModel(Project project)
    {
        _project = project;
    }

    public ProjectViewModel()
    {
    }

    public async Task LoadIcon()
    {
        if (string.IsNullOrEmpty(_project.IconPath))
            return;
        Icon = await Task.Run(() => new Bitmap(_project.IconPath));
    }
}
using System;
using System.IO;
using Avalonia.Media.Imaging;
using ReactiveUI;
using Seed.Models;
using Task = System.Threading.Tasks.Task;

namespace Seed.ViewModels;

public class ProjectViewModel: ViewModelBase
{
    private Project? _project;
    private Bitmap? _icon;
    
    public string? Name => _project?.Name;
    
    public Version? EngineVersion => _project?.Version;

    public Bitmap? Icon
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
        _project = new Project("Big AAA Game", "/home/user/dev/projects/Big AAA Game", new Version(1, 6, 6032, 4));
    }

    public async Task LoadIcon()
    {
        if (_project is null)
            return;
        if (string.IsNullOrEmpty(_project.IconPath))
            return;
        
        Icon = await Task.Run(() => new Bitmap(_project.IconPath));
    }
}
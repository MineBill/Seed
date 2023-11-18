using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using Seed.Models;

namespace Seed.ViewModels;

public class TemplateViewModel : ViewModelBase
{
    public ProjectTemplate ProjectTemplate { get; }

    public Task<Bitmap> Icon => ProjectTemplate.GetIcon();

    public string Name => ProjectTemplate.Name;

    public EngineVersion EngineVersion => ProjectTemplate.GetEngineVersion();

    public TemplateViewModel(ProjectTemplate projectTemplate)
    {
        ProjectTemplate = projectTemplate;
    }
}
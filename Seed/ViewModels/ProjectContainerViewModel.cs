using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;
using Seed.Services;

namespace Seed.ViewModels;

// TODO: Refactor ProjectsViewModel to use this.
public class ProjectContainerViewModel : ReactiveValidationObject
{
    private ProjectViewModel? _selectedProject;

    public ProjectViewModel? SelectedProject
    {
        get => _selectedProject;
        set => this.RaiseAndSetIfChanged(ref _selectedProject, value);
    }

    public ObservableCollection<ProjectViewModel> Projects { get; } = new();

    public ProjectContainerViewModel(IProjectManager projectManager)
    {
        foreach (var project in projectManager.Projects.Where(x => x.IsTemplate))
        {
            Projects.Add(new ProjectViewModel(project));
        }
    }
}
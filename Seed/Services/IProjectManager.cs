using System.Collections.ObjectModel;
using Seed.Models;

namespace Seed.Services;

public interface IProjectManager
{
    /// <summary>
    /// The list of known projects. Useful to subscribe to updates.
    /// </summary>
    public ObservableCollection<Project> Projects { get; }

    /// <summary>
    /// Add a new project to the list of known projects.
    /// Will immediately save the known project to the filesystem.
    /// </summary>
    /// <param name="project"></param>
    public void AddProject(Project project);

    /// <summary>
    /// Remove a project from the list of known projects.
    /// Will immediately save the known projects list to the filesystem.
    /// </summary>
    /// <param name="project"></param>
    public void RemoveProject(Project project);

    /// <summary>
    /// Launches the project.
    /// </summary>
    /// <param name="project"></param>
    public void RunProject(Project project);

    /// <summary>
    /// Save the projects in <see cref="Projects"/>.
    /// </summary>
    public void Save();
}
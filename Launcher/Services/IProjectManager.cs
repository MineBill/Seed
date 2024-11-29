using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Launcher.DataModels;

namespace Launcher.Services;

public interface IProjectManager
{
    public delegate void SaveEvent();

    public event SaveEvent OnSaved;

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
    /// Tries to add a project from an existing path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Project? ParseProject(string path);

    public Task<Project?> AddProjectFromGitRepo(string repoUrl, string destination);

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
    /// Deletes the Cache folder of a project.
    /// </summary>
    /// <param name="project"></param>
    public void ClearCache(Project project);

    /// <summary>
    /// Save the projects in <see cref="Projects"/>.
    /// </summary>
    public void Save();
}
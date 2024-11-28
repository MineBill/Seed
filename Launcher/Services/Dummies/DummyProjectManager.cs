using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Launcher.DataModels;

namespace Launcher.Services.Dummies;

public class DummyProjectManager : IProjectManager
{
#pragma warning disable 67
    public event IProjectManager.SaveEvent? OnSaved;
#pragma warning restore 67

    public ObservableCollection<Project> Projects { get; } = [];

    public void AddProject(Project project)
    {
    }

    Project? IProjectManager.TryAddProject(string path)
    {
        throw new System.NotImplementedException();
    }

    public Task<Project?> AddProjectFromGitRepo(string repoUrl, string destination)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveProject(Project project)
    {
    }

    public void RunProject(Project project)
    {
    }

    public void ClearCache(Project project)
    {
    }

    public void Save()
    {
    }
}
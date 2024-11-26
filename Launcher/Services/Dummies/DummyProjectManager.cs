using System.Collections.ObjectModel;
using Launcher.DataModels;

namespace Launcher.Services.Dummies;

public class DummyProjectManager : IProjectManager
{
    public event IProjectManager.SaveEvent? OnSaved;
    public ObservableCollection<Project> Projects { get; } = [];

    public void AddProject(Project project)
    {
    }

    public bool TryAddProject(string path)
    {
        return false;
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
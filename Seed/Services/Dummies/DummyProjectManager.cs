using System.Collections.ObjectModel;
using Seed.Models;

namespace Seed.Services.Dummies;

public class DummyProjectManager : IProjectManager
{
    public ObservableCollection<Project> Projects =>
        new()
        {
            new Project("Pepegidas", "/home/minebill/ProjectFolder"),
            new Project("Clapex Logends", @"C:\User\Fuck\GameFolder")
        };

    public void AddProject(Project project)
    {
    }

    public void RemoveProject(Project project)
    {
    }

    public void RunProject(Project project)
    {
    }

    public void Save()
    {
    }
}
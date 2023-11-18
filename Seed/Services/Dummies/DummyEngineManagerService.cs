using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Seed.Models;

namespace Seed.Services.Dummies;

public class DummyEngineManagerService : IEngineManager
{
    public ObservableCollection<Engine> Engines
    {
        get
        {
            return new ObservableCollection<Engine>()
            {
                new()
                {
                    Path = "/home/minebill/.local/share/Seed/Installs/Flax_1.7",
                    Version = new NormalVersion(new Version(1, 7, 6045, 0))
                },
                new()
                {
                    Path = @"C:\Program Files (x86)\Flax\Flax_1.7",
                    Version = new NormalVersion(new Version(1, 7, 6045, 3))
                }
            };
        }
    }

    public List<Engine> GetInstalledEngines()
    {
        return new List<Engine>
        {
            new()
            {
                Path = "/home/minebill/.local/share/Seed/Installs/Flax_1.7",
                Version = new NormalVersion(new Version(1, 7, 6045, 0))
            },
            new()
            {
                Path = @"C:\Program Files (x86)\Flax\Flax_1.7",
                Version = new NormalVersion(new Version(1, 7, 6045, 3))
            }
        };
    }

    public void AddEngine(Engine engine)
    {
    }

    public void DeleteEngine(Engine engine)
    {
    }

    public void CreateProject(Project newProject, Project template)
    {
    }

    public void CreateProject(Engine engine, Project project, bool useBasicScene)
    {
    }

    public void CreateProject(Engine engine, Project newProject, Project template)
    {
    }
}
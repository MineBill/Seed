using System.Collections.Generic;
using Seed.Models;

namespace Seed.Services;

public interface IProjectLocatorService
{
    public List<Project> GetProjects();
}
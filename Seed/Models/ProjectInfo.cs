using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Seed.Models;

public class ProjectInfo
{
    public List<Project> Projects { get; init; } = new();
}
using System;
using Seed.Models;

namespace Seed.ViewModels.Mock;

public class MockProjectViewModel: ProjectViewModel
{
    public new string? Name => "Flax Demo Project";

    public new Version? EngineVersion => new (1, 9, 6053, 1);
}
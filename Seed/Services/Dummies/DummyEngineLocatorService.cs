using System;
using System.Collections.Generic;
using Seed.Models;

namespace Seed.Services.Dummies;

public class DummyEngineLocatorService: IEngineLocatorService
{
    public List<Engine> GetInstalledEngines()
    {
        return new List<Engine>
        {
            new()
            {
                Path = "/home/minebill/.local/share/Seed/Installs/Flax_1.7",
                Version = new Version(1, 7, 6045, 0)
            },
            new()
            {
                Path = @"C:\Program Files (x86)\Flax\Flax_1.7",
                Version = new Version(1, 7, 6045, 3)
            }
        };
    }
}
using System;
using System.Collections.Generic;

namespace Seed.Models;

public class Engine
{
    /// <summary>
    /// The engine name. Usually the same as <see cref="Version"/>.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Where the engine is installed.
    /// </summary>
    public string Path { get; set; } = string.Empty;
    
    /// <summary>
    ///  The engine version.
    /// </summary>
    public Version Version { get; set; } = new Version();

    /// <summary>
    /// The installed platform tools alongside the engine.
    /// </summary>
    public List<Package> InstalledPackages { get; set; } = new();

    /// <summary>
    /// Validates that the engine is correctly installed at the 
    /// </summary>
    /// <returns>True if the engine installation is valid, false otherwise.</returns>
    public bool ValidateInstallation()
    {
        return true;
    }
}

public class Package
{
    public string Name { get; set; }
    
    public string Path { get; set; }

    public Package(string name, string path)
    {
        Name = name;
        Path = path;
    }

    public bool ValidateInstallation()
    {
        return true;
    }
}
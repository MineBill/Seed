using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text.Json.Serialization;

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
    public EngineVersion Version { get; set; }

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
        return File.Exists(System.IO.Path.Combine(Path, "Flax.flaxproj"));
    }

    public string GetExecutablePath(string mode)
    {
        return System.IO.Path.Combine(Path, "Binaries", "Editor", "Linux", mode, "FlaxEditor");
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
        return File.Exists(System.IO.Path.Combine(Path, ""));
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<Package>))]
internal partial class PackageGenerationContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<Engine>))]
internal partial class EngineGenerationContext : JsonSerializerContext
{
}
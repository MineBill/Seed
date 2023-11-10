using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Seed.Models;

/// <summary>
/// The project structure used in the launcher and save file. Does not
/// reflect the actual .flaxproj file.
/// </summary>
/// <param name="Name">The name of the project.</param>
/// <param name="Path">Where the project is located in the filesystem.</param>
/// <param name="EngineVersion">The associated engine version with this project.</param>
public record Project(string Name, string Path, Version? EngineVersion = null)
{
    /// <summary>
    /// The name of the project.
    /// </summary>
    public string Name { get; set; } = Name;

    /// <summary>
    /// Where the project is located in the filesystem.
    /// </summary>
    public string Path { get; set; } = Path;

    /// <summary>
    /// The associated engine version with this project.
    /// </summary>
    public Version? EngineVersion { get; set; } = EngineVersion;

    /// <summary>
    /// Utility to get the project
    /// </summary>
    public string IconPath => System.IO.Path.Combine(Path, "Cache", "icon.png");

    /// <summary>
    /// Last time the project was opened from the launcher.
    /// </summary>
    public DateTime LastOpenedTime { get; set; }

    /// <summary>
    /// Check if this project exists on disk right now.
    /// </summary>
    /// <returns></returns>
    public bool ValidateInstallation()
    {
        return File.Exists(System.IO.Path.Combine(Path, $"{Name}.flaxproj"));
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<Project>))]
internal partial class ProjectGenerationContext : JsonSerializerContext
{
}
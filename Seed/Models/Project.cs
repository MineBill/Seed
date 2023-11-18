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
public record Project(string Name, string Path, EngineVersion? EngineVersion = null)
{
    /// <summary>
    /// Utility to get the project
    /// </summary>
    [JsonIgnore]
    public string IconPath => System.IO.Path.Combine(Path, "Cache", "icon.png");

    /// <summary>
    /// Last time the project was opened from the launcher.
    /// </summary>
    public DateTime LastOpenedTime { get; set; }

    /// <summary>
    /// Indicates whether this project can be used as a template when creating a new project.
    /// </summary>
    public bool IsTemplate { get; set; }

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
using System.Text.Json.Serialization;
using Avalonia.Media;

namespace Seed.Models;

/// <summary>
/// Stores user preferences.
/// </summary>
public class UserPreferences
{
    /// <summary>
    /// The engine installation folder.
    /// </summary>
    public string? EngineInstallLocation { get; set; } = string.Empty;

    /// <summary>
    /// The color used to highlight projects as templates.
    /// </summary>
    public Color? TemplateProjectOutline { get; set; } = Colors.Aqua;

    /// <summary>
    /// The default project location for new project. Can be overriden in the project creation dialog.
    /// </summary>
    public string? NewProjectLocation { get; set; } = string.Empty;
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(UserPreferences))]
internal partial class UserPreferencesGenerationContext : JsonSerializerContext
{
}
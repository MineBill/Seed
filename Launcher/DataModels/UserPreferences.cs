using System.Text.Json.Serialization;
using Avalonia.Media;

namespace Launcher.DataModels;

public enum SortingType
{
    Name,
    OpenDate,
    EngineVersion,
}

public enum SortingDirection
{
    Ascending,
    Descending,
}

/// <summary>
/// Stores user preferences.
/// </summary>
public class UserPreferences
{
    /// <summary>
    /// The engine installation folder.
    /// </summary>
    public string? EngineInstallLocation { get; set; }

    /// <summary>
    /// The color used to highlight projects as templates.
    /// </summary>
    public Color? TemplateProjectOutline { get; set; } = Colors.Aqua;

    /// <summary>
    /// The default project location for new project. Can be overriden in the project creation dialog.
    /// </summary>
    public string? NewProjectLocation { get; set; }

    /// <summary>
    /// The access token used to authenticate with GitHub for downloads.
    /// </summary>
    public string? GithubAccessToken { get; set; }

    /// <summary>
    /// The theme type.
    /// </summary>
    public string ColorTheme { get; set; } = "Auto";

    /// <summary>
    /// The user-selected accent color. If null, use the system accent color.
    /// </summary>
    public Color? AccentColor { get; set; }

    /// <summary>
    /// Don't check for updates during start-up.
    /// </summary>
    public bool SkipUpdates { get; set; }

    public SortingType ProjectSortingType { get; set; } = SortingType.OpenDate;

    public SortingDirection ProjectSortingDirection { get; set; } = SortingDirection.Descending;
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(UserPreferences))]
internal partial class UserPreferencesGenerationContext : JsonSerializerContext;
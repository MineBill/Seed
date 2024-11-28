using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Launcher.DataModels;

/// <summary>
/// A GitHub artifact. Used to download daily engine builds from continuous deployment.
/// </summary>
public class GitHubArtifact
{
    /// <summary>
    /// The id of this artifact.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// The name of the artifact, as displayed on the website.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The url to download this artifact. Needs an access token.
    /// </summary>
    [JsonPropertyName("archive_download_url")]
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Is this artifact expired?
    /// </summary>
    [JsonPropertyName("expired")]
    public bool Expired { get; set; }

    /// <summary>
    /// The size of the download. Used to track download progress.
    /// </summary>
    [JsonPropertyName("size_in_bytes")]
    public long SizeInBytes { get; set; }

    /// <summary>
    /// The target path this artifact should be extracted to.
    /// </summary>
    [JsonIgnore]
    public string TargetPath { get; set; } = string.Empty;

    /// <summary>
    /// The commit that was used to produce this artifact.
    /// </summary>
    [JsonIgnore]
    public string CommitHash { get; set; } = string.Empty;

    /// <summary>
    /// The operating system this artifact is for.
    /// </summary>
    [JsonIgnore]
    public OSPlatform OperatingSystem { get; set; }

    /// <summary>
    /// Is this artifact an editor package?
    /// </summary>
    [JsonIgnore]
    public bool IsEditor { get; set; }

    /// <summary>
    /// Returns if this artifact is meant for the running operating system.
    /// </summary>
    public bool IsForThisPlatform()
    {
        if (System.OperatingSystem.IsWindows())
            return OperatingSystem == OSPlatform.Windows;
        if (System.OperatingSystem.IsLinux() || System.OperatingSystem.IsFreeBSD())
            return OperatingSystem == OSPlatform.Linux;
        return false;
    }

    /// <summary>
    /// Returns if this artifact is meant for the running operating system.
    /// </summary>
    public bool SupportsThisPlatform()
    {
        if (System.OperatingSystem.IsWindows())
            return OperatingSystem == OSPlatform.Linux || OperatingSystem == OSPlatform.Windows;
        if (System.OperatingSystem.IsLinux() || System.OperatingSystem.IsFreeBSD())
            return OperatingSystem == OSPlatform.Linux;
        return false;
    }
}

[JsonSerializable(typeof(GitHubArtifact))]
internal partial class GitHubArtifactGenerationContext : JsonSerializerContext;
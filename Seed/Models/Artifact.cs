using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Seed.Models;

/// <summary>
/// A github artifact. Used to download daily engine builds from continuous deployment.
/// </summary>
public class Artifact
{
    /// <summary>
    /// The id of this artifact.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("archive_download_url")]
    public string DownloadUrl { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("expired")]
    public bool Expired { get; set; }

    [JsonPropertyName("size_in_bytes")]
    public long SizeInBytes { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonIgnore]
    public string TargetPath { get; set; } = string.Empty;

    /// <summary>
    /// The commit that was used to produce this artifact.
    /// </summary>
    [JsonIgnore]
    public string CommitHash { get; set; } = string.Empty;

    /// <summary>
    ///
    ///
    /// </summary>
    [JsonIgnore]
    public OSPlatform OperatingSystem { get; set; }

    [JsonIgnore]
    public bool IsEditor { get; set; }

    public bool IsForThisPlatform()
    {
        if (System.OperatingSystem.IsWindows())
            return OperatingSystem == OSPlatform.Linux || OperatingSystem == OSPlatform.Windows;
        if (System.OperatingSystem.IsLinux() || System.OperatingSystem.IsFreeBSD())
            return OperatingSystem == OSPlatform.Linux;
        return false;
    }
}
// TODO: Use a JsonSerializerContext to avoid some reflection.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Launcher.DataModels;

/// <summary>
/// Describes a remote engine as described by the api the official Flax launcher uses.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class RemoteEngine : IComparable<RemoteEngine>
{
    /// <summary>
    /// The name of the engine.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The version of the engine.
    /// </summary>
    [JsonPropertyName("version")]
    public Version Version { get; set; } = new Version();

    /// <summary>
    /// The packages available for this engine version.
    /// </summary>
    [JsonPropertyName("packages")]
    public List<RemotePackage> Packages { get; set; } = new();

    public List<RemotePackage> SupportedPlatformTools => Packages.FindAll(x => !x.IsEditorPackage && CanBuildFor(x));

    /// <summary>
    /// Get the editor package for this engine.
    /// </summary>
    /// <returns></returns>
    public RemotePackage GetEditorPackage()
    {
        return Packages.First(x => x.IsEditorPackage);
    }

    public int CompareTo(RemoteEngine? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return Version.CompareTo(other.Version);
    }

    private static bool CanBuildFor(RemotePackage package)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return package.IsWindowsTools || package.IsLinuxTools || package.IsAndroidTools;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            return package.IsLinuxTools || package.IsAndroidTools;
        }

        return false;
    }
}


[JsonSerializable(typeof(RemoteEngine))]
internal partial class RemoteEngineGenerationContext : JsonSerializerContext;

[JsonSerializable(typeof(List<RemoteEngine>))]
internal partial class RemoteEngineListGenerationContext : JsonSerializerContext;
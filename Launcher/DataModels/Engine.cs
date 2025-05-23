using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using NLog;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace Launcher.DataModels;

public class Engine
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public enum Configuration
    {
        Debug = 0,
        Development = 1,
        Release = 2
    }

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
    public required EngineVersion Version { get; set; }

    [JsonIgnore]
    public string DisplayName => $"{Name} - {Version}";

    [JsonIgnore]
    public string DisplayNameShort =>
        Version switch
        {
            GitVersion gitVersion => $"CI {gitVersion.Commit[..5]} {gitVersion.CreationTime.ToShortDateString()}",
            LocalBuild localBuild => $"Local {localBuild.Version}",
            NormalVersion => Version.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(Version))
        };

    /// <summary>
    /// The preferred configuration to use when selecting the editor executable.
    /// </summary>
    public Configuration PreferredConfiguration { get; set; } = Configuration.Release;

    /// <summary>
    /// Which configurations ara available for this engine.
    /// </summary>
    public List<Configuration> AvailableConfigurations { get; set; } = new();

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
        // TODO: Also check if the all of the available configurations are present.
        return File.Exists(System.IO.Path.Combine(Path, "Flax.flaxproj"));
    }

    /// <summary>
    /// Check and mark that the engine binaries are marked as executables on linux systems.
    /// </summary>
    public void EnsureMarkedExecutable()
    {
        if (OperatingSystem.IsWindows())
            return;

        try
        {
            var toolPath = System.IO.Path.Combine(Path, "Binaries", "Tools", "Flax.Build");
            Process.Start("chmod", $"+x \"{toolPath}\"");

            foreach (var config in AvailableConfigurations)
            {
                var path = System.IO.Path.Combine(Path, "Binaries", "Editor", "Linux", config.ToString(), "FlaxEditor");
                Process.Start("chmod", $"+x \"{path}\"");
            }
        }
        catch (FileNotFoundException fnf)
        {
            Logger.Error(fnf, "Failed to mark binary as executable");
        }
    }

    public string GetExecutablePath(Configuration configuration)
    {
        var os = string.Empty;
        var exe = string.Empty;
        if (OperatingSystem.IsWindows())
        {
            os = "Win64";
            exe = "FlaxEditor.exe";
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
        {
            os = "Linux";
            exe = "FlaxEditor";
        }
        else if (OperatingSystem.IsMacOS())
        {
            os = "MacOS"; // TODO: Is this the correct folder name for macs?
            exe = "FlaxEditor";
        }

        return System.IO.Path.Combine(Path, "Binaries", "Editor", os, configuration.ToString(), exe);
    }
}

public class Package(string name, string path)
{
    public string Name { get; set; } = name;

    public string Path { get; set; } = path;

    public bool ValidateInstallation()
    {
        return File.Exists(System.IO.Path.Combine(Path, ""));
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Package))]
internal partial class PackageGenerationContext : JsonSerializerContext;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Engine))]
internal partial class EngineGenerationContext : JsonSerializerContext;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<Package>))]
internal partial class PackageListGenerationContext : JsonSerializerContext;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<Engine>))]
internal partial class EngineListGenerationContext : JsonSerializerContext;
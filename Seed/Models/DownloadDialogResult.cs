using System.Collections.Generic;

namespace Seed.Models;

/// <summary>
/// Helper class the contains the result of the download dialog.
/// </summary>
public class DownloadDialogResult<C, T>
{
    public C Engine { get; set; }

    /// <summary>
    /// Which platform tools to install.
    /// </summary>
    public List<T> PlatformTools { get; set; }

    public DownloadDialogResult(C engine, List<T> tools)
    {
        Engine = engine;
        PlatformTools = tools;
    }
}

public class ArtifactsDownloadResult
{
    public Artifact Engine { get; set; }

    /// <summary>
    /// Which platform tools to install.
    /// </summary>
    public List<Artifact> PlatformTools { get; set; }

    public ArtifactsDownloadResult(Artifact engine, List<Artifact> tools)
    {
        Engine = engine;
        PlatformTools = tools;
    }
}
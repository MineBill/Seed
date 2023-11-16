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
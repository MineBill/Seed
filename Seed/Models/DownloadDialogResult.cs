using System.Collections.Generic;

namespace Seed.Models;

/// <summary>
/// Helper class the contains the result of the download dialog.
/// </summary>
public class DownloadDialogResult
{
    public RemoteEngine Engine { get; set; }

    /// <summary>
    /// Which platform tools to install.
    /// </summary>
    public List<RemotePackage> PlatformTools { get; set; }

    public DownloadDialogResult(RemoteEngine engine, List<RemotePackage> tools)
    {
        Engine = engine;
        PlatformTools = tools;
    }
}
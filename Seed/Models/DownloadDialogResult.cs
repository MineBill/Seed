using System.Collections.Generic;

namespace Seed.Models;

/// <summary>
/// Helper class the contains the result of the download dialog.
/// </summary>
public class DownloadDialogResult
{
    public RemoteEngine Engine { get; set; }
    
    /// <summary>
    /// The editor package to install.
    /// </summary>
    public RemotePackage Editor { get; set; }

    /// <summary>
    /// Which platform tools to install.
    /// </summary>
    public List<RemotePackage> PlatformTools { get; set; }

    public DownloadDialogResult(RemoteEngine engine, RemotePackage editor, List<RemotePackage> tools)
    {
        Engine = engine;
        Editor = editor;
        PlatformTools = tools;
    }
}
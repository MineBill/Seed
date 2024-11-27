using System;
using System.Collections.Generic;

namespace Launcher.Services.DefaultImplementations;

public class DownloadManager : IDownloadManager
{
    private readonly List<DownloadEntry> _downloads = [];

    public event Action<DownloadEntry>? EntryAdded;
    public event Action<DownloadEntry>? EntryRemoved;

    public void AddDownload(DownloadEntry entry)
    {
        _downloads.Add(entry);
        EntryAdded?.Invoke(entry);
    }

    public void RemoveDownload(DownloadEntry entry)
    {
        _downloads.Remove(entry);
        EntryRemoved?.Invoke(entry);
    }
}
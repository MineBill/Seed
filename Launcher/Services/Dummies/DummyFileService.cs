using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Launcher.Services.Dummies;

public class DummyFileService : IFilesService
{
    public Task<IStorageFile?> SelectFileAsync(string title, IReadOnlyList<FilePickerFileType>? options = default)
    {
        return Task.FromResult<IStorageFile?>(null);
    }

    public Task<IStorageFolder?> SelectFolderAsync(string? path = null)
    {
        return Task.FromResult<IStorageFolder?>(null);
    }

    public void OpenFolder(string path)
    {
    }

    public void OpenUri(Uri uri)
    {
    }
}
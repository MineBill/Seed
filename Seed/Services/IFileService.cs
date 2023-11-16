using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Seed.Services;

public interface IFilesService
{
    /// <summary>
    /// Open a file dialog and return the selected file(s).
    /// </summary>
    /// <param name="title">Title for the dialog window.</param>
    /// <param name="options">Options to pass to the window.</param>
    /// <returns></returns>
    public Task<IStorageFile?> SelectFileAsync(string title, IReadOnlyList<FilePickerFileType> options);

    /// <summary>
    /// Open a file dialog and return the selected folder(s).
    /// </summary>
    /// <param name="path">Optional path to start the dialog at.</param>
    /// <returns></returns>
    public Task<IStorageFolder?> SelectFolderAsync(string? path = null);

    /// <summary>
    /// Open the system file explorer at the specified location.
    /// </summary>
    /// <param name="path">The location to open the explorer at.</param>
    public void OpenFolder(string path);

    /// <summary>
    /// Open the uri on the system default browser.
    /// </summary>
    /// <param name="uri">The uri to open</param>
    public void OpenUri(Uri uri);
}
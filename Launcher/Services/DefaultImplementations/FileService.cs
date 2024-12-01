using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Launcher.Services.DefaultImplementations;

public class FilesService(Window target) : IFilesService
{
    /// <inheritdoc />
    public async Task<IStorageFile?> SelectFileAsync(string title, IReadOnlyList<FilePickerFileType>? options = default)
    {
        var files = await target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = options
        });

        return files.Count >= 1 ? files[0] : null;
    }

    public async Task<IStorageFile?> SaveFileAsync()
    {
        return await target.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Save Text File"
        });
    }

    public async Task<IStorageFolder?> SelectFolderAsync(string? path = null)
    {
        var folders = await target.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
            SuggestedStartLocation =
                await target.StorageProvider.TryGetFolderFromPathAsync(path ??
                                                                       Environment.GetFolderPath(Environment
                                                                           .SpecialFolder.ApplicationData))
        });
        return folders.Count > 0 ? folders[0] : null;
    }

    /// <inheritdoc />
    public void OpenFolder(string path)
    {
        var info = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsLinux() ? "xdg-open" : "explorer.exe",
            ArgumentList = { path },
        };
        Process.Start(info);
    }

    /// <inheritdoc />
    public void OpenUri(Uri uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = uri.ToString(),
            });
        }
        catch
        {
            if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", uri.ToString());
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", uri.ToString());
            }
            else
            {
                throw;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Seed.Services.Implementations;

public class FilesService : IFilesService
{
    private readonly Window _target;

    public FilesService(Window target)
    {
        _target = target;
    }

    public async Task<IStorageFile?> OpenFileAsync(string title, IReadOnlyList<FilePickerFileType> options)
    {
        var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = options
        });

        return files.Count >= 1 ? files[0] : null;
    }

    public async Task<IStorageFile?> SaveFileAsync()
    {
        return await _target.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Save Text File"
        });
    }

    public void OpenFolder(string path)
    {
        var info = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsLinux() ? "xdg-open" : "explorer.exe",
            ArgumentList = { path },
        };
        Process.Start(info);
    }
}
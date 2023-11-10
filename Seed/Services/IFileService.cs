using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Seed.Services;

public interface IFilesService
{
    public Task<IStorageFile?> OpenFileAsync(string title, IReadOnlyList<FilePickerFileType> options);
    public Task<IStorageFile?> SaveFileAsync();

    public void OpenFolder(string path);
}
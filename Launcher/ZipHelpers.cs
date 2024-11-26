using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Launcher;

public static class ZipHelpers
{
    public static async Task ExtractToDirectoryAsync(
        string pathZip,
        string pathDestination,
        IProgress<float> progress,
        CancellationToken cancellationToken = default)
    {
        using var archive = ZipFile.OpenRead(pathZip);
        var totalLength = archive.Entries.Sum(entry => entry.Length);
        long currentProgression = 0;
        foreach (var entry in archive.Entries)
        {
            // Check if entry is a folder
            var filePath = Path.Combine(pathDestination, entry.FullName);
            if (entry.FullName.EndsWith('/') || entry.FullName.EndsWith('\\'))
            {
                Directory.CreateDirectory(filePath);
                continue;
            }

            // Create folder anyway since a folder may not have an entry
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            await using var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await using (var entryStream = entry.Open())
            {
                var progression = currentProgression;
                var relativeProgress = new Progress<long>(fileProgressBytes =>
                    progress.Report((float)(fileProgressBytes + progression) / totalLength));
                await entryStream.CopyToAsync(file, 81920, relativeProgress, cancellationToken);
            }

            if (OperatingSystem.IsLinux())
                ExtractExternalAttributes(file, entry);

            currentProgression += entry.Length;
        }
    }

    public static async Task ExtractToDirectoryAsync(
        Stream zipStream,
        string pathDestination,
        IProgress<float> progress,
        CancellationToken cancellationToken = default)
    {
        using var archive = new ZipArchive(zipStream);
        var totalLength = archive.Entries.Sum(entry => entry.Length);
        long currentProgression = 0;
        foreach (var entry in archive.Entries)
        {
            // Check if entry is a folder
            string filePath = Path.Combine(pathDestination, entry.FullName);
            if (entry.FullName.EndsWith('/') || entry.FullName.EndsWith('\\'))
            {
                Directory.CreateDirectory(filePath);
                continue;
            }
    
            // Create folder anyway since a folder may not have an entry
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            await using var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await using (var entryStream = entry.Open())
            {
                var progression = currentProgression;
                var relativeProgress = new Progress<long>(fileProgressBytes =>
                    progress.Report((float)(fileProgressBytes + progression) / totalLength));
                await entryStream.CopyToAsync(file, 81920, relativeProgress, cancellationToken);
            }
    
            if (OperatingSystem.IsLinux())
                ExtractExternalAttributes(file, entry);
    
            currentProgression += entry.Length;
        }
    }

    // https://github.com/dotnet/runtime/pull/55531/files
    [SupportedOSPlatform("linux")]
    private static void ExtractExternalAttributes(FileStream fs, ZipArchiveEntry entry)
    {
        // Only extract USR, GRP, and OTH file permissions, and ignore
        // S_ISUID, S_ISGID, and S_ISVTX bits. This matches unzip's default behavior.
        // It is off by default because of this comment:

        // "It's possible that a file in an archive could have one of these bits set
        // and, unknown to the person unzipping, could allow others to execute the
        // file as the user or group. The new option -K bypasses this check."
        const int extractPermissionMask = 0x1FF;
        var permissions = (entry.ExternalAttributes >> 16) & extractPermissionMask;

        // If the permissions weren't set at all, don't write the file's permissions,
        // since the .zip could have been made using a previous version of .NET, which didn't
        // include the permissions, or was made on Windows.
        if (permissions != 0)
        {
            fchmod(fs.SafeFileHandle.DangerousGetHandle().ToInt32(), permissions);
        }
    }

    [SupportedOSPlatform("linux")]
    [DllImport("libc", SetLastError = true)]
    static extern void fchmod(int fd, int mode);
}
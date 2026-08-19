using System;
using NLog;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Launcher;

public static class DmgHelpers
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    [SupportedOSPlatform("OSX")]
    public static async Task<string> MountToDirectoryAsync(
        string dmgPath,
        CancellationToken cancellationToken = default)
    {
        var mountPoint = Path.Combine(
            Path.GetTempPath(),
            $"SeedDmg_{Path.GetRandomFileName()}");

        Directory.CreateDirectory(mountPoint);

        var startInfo = new ProcessStartInfo
        {
            FileName = "hdiutil",
            Arguments = $"attach \"{dmgPath}\" -nobrowse -mountpoint \"{mountPoint}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = await process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            try
            {
                Directory.Delete(mountPoint, true);
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Failed to remove temporary DMG mount point: {0}", mountPoint);
            }

            throw new InvalidOperationException(
                $"Failed to mount DMG: {error}");
        }

        return mountPoint;
    }

    [SupportedOSPlatform("OSX")]
    public static async Task ExtractToDirectoryAsync(
        string pathDmg,
        string pathDestination,
        IProgress<float>? progress,
        CancellationToken cancellationToken = default)
    {
        var mountPoint = await MountToDirectoryAsync(
            pathDmg,
            cancellationToken);

        try
        {
            Directory.CreateDirectory(pathDestination);

            var totalSize = GetDirectorySize(mountPoint);
            long copiedSize = 0;

            await CopyDirectoryAsync(
                mountPoint,
                pathDestination,
                totalSize,
                progress,
                copiedSize,
                cancellationToken);
        }
        finally
        {
            await UnmountAsync(mountPoint, cancellationToken);
        }
    }

    private static long GetDirectorySize(string directory)
    {
        long size = 0;

        foreach (var file in Directory.EnumerateFiles(
                     directory,
                     "*",
                     SearchOption.AllDirectories))
        {
            size += new FileInfo(file).Length;
        }

        return size;
    }

    private static async Task CopyDirectoryAsync(
        string sourceDirectory,
        string destinationDirectory,
        long totalSize,
        IProgress<float>? progress,
        long copiedSize,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(destinationDirectory);

        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var directoryName = Path.GetFileName(directory);
            var destination = Path.Combine(
                destinationDirectory,
                directoryName);

            await CopyDirectoryAsync(
                directory,
                destination,
                totalSize,
                progress,
                copiedSize,
                cancellationToken);
        }

        foreach (var sourceFile in Directory.EnumerateFiles(sourceDirectory))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = Path.GetFileName(sourceFile);
            var destinationFile = Path.Combine(
                destinationDirectory,
                fileName);

            var fileLength = new FileInfo(sourceFile).Length;

            await using var source = new FileStream(
                sourceFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            await using var destination = new FileStream(
                destinationFile,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None);

            var buffer = new byte[81920];
            int bytesRead;

            while ((bytesRead = await source.ReadAsync(
                       buffer,
                       cancellationToken)) > 0)
            {
                await destination.WriteAsync(
                    buffer.AsMemory(0, bytesRead),
                    cancellationToken);

                copiedSize += bytesRead;

                if (totalSize > 0)
                {
                    progress?.Report(
                        (float)copiedSize / totalSize);
                }
            }
        }
    }

    [SupportedOSPlatform("OSX")]
    private static async Task UnmountAsync(
        string mountPoint,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "hdiutil",
            Arguments = $"detach \"{mountPoint}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        var error = await process.StandardError.ReadToEndAsync(
            cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Failed to unmount DMG: {error}");
        }
    }
}

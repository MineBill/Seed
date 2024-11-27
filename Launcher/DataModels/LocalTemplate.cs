using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Launcher.DataModels;

/// <summary>
/// A template that is another project stored on the disk.
/// </summary>
public class LocalTemplate : ProjectTemplate
{
    public Project Project { get; }

    public LocalTemplate(Project project)
    {
        Project = project;
        Name = Project.Name;
    }

    /// <inheritdoc/>
    public override int Order => 1;

    /// <inheritdoc/>
    public override string Name { get; }

    /// <inheritdoc/>
    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names")]
    public override async Task<Project?> Create(string name, string parentFolder, Engine engine)
    {
        var projectFolder = Path.Combine(parentFolder, name);
        CopyDirectory(Project.Path, projectFolder, true);

        // We prefer to delete unneeded folders instead of only copying the needed ones
        // because we don't know if this template projects has extra folders that are needed.
        Directory.Delete(Path.Combine(projectFolder, "Cache"), recursive: true);
        Directory.Delete(Path.Combine(projectFolder, "Binaries"), recursive: true);

        var flaxProj = Path.GetRelativePath(Project.Path, Project.FlaxProj);
        var jsonText = await File.ReadAllTextAsync(Project.FlaxProj);
        var json = JsonNode.Parse(jsonText);
        if (json != null)
            json["Name"] = name;

        await using (var file = new FileStream(Project.FlaxProj, FileMode.Create))
        {
            await using var writer = new Utf8JsonWriter(file, new JsonWriterOptions
            {
                Indented = true
            });
            json?.WriteTo(writer);
        }

        var newFlaxProj = Path.Combine(projectFolder, name + ".flaxproj");
        File.Move(Path.Combine(projectFolder, flaxProj), newFlaxProj);

        return new Project(name, projectFolder, FlaxProj: newFlaxProj, engine.Version);
    }

    /// <inheritdoc/>
    public override Task<Bitmap> Icon
    {
        get
        {
            if (!File.Exists(Project.IconPath))
                return Task.Run(() =>
                    new Bitmap(AssetLoader.Open(new Uri("avares://Launcher/Assets/project_icon.png"))));

            return Task.Run(() =>
            {
                using var file = new FileStream(Project.IconPath, FileMode.Open, FileAccess.Read);
                return new Bitmap(file);
            });
        }
    }

    public override string Description => string.Empty;

    public override EngineVersion? MinimumEngineVersion => Project.EngineVersion;

    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        var dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (var subDir in dirs)
            {
                var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}
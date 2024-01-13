using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Markdown.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Seed.Services;

namespace Seed.Models.ProjectTemplates;

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
    public override async void Create(Project newProject)
    {
        CopyDirectory(Project.Path, newProject.Path, true);

        // We prefer to delete unneeded folders instead of only copying the needed ones
        // because we don't know if this template projects has extra folders that are needed.
        Directory.Delete(Path.Combine(newProject.Path, "Cache"), recursive: true);
        Directory.Delete(Path.Combine(newProject.Path, "Binaries"), recursive: true);

        var flaxproj = Path.Combine(newProject.Path, Project.Name) + ".flaxproj";
        var jsonText = await File.ReadAllTextAsync(flaxproj);
        var json = JsonNode.Parse(jsonText);
        if (json != null)
            json["Name"] = newProject.Name;

        await using (var file = new FileStream(flaxproj, FileMode.Create))
        {
            await using var writer = new Utf8JsonWriter(file, new JsonWriterOptions
            {
                Indented = true
            });
            json?.WriteTo(writer);
        }

        File.Move(flaxproj, Path.Combine(newProject.Path, newProject.Name + ".flaxproj"));

        var projectManager = App.Current.Services.GetService<IProjectManager>();
        projectManager?.AddProject(newProject);
    }

    /// <inheritdoc/>
    public override async Task<Bitmap> GetIcon()
    {
        if (!File.Exists(Project.IconPath))
            return await Task.Run(() =>
                new Bitmap(AssetLoader.Open(new Uri("avares://Seed/Assets/avalonia-logo.ico"))));

        return await Task.Run(() =>
        {
            using var file = new FileStream(Project.IconPath, FileMode.Open, FileAccess.Read);
            return new Bitmap(file);
        });
    }

    public override EngineVersion GetEngineVersion()
    {
        return Project.EngineVersion!;
    }

    static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
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
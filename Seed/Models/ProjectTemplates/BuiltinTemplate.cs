using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Seed.Services;

namespace Seed.Models.ProjectTemplates;

/// <summary>
/// A builtin, compressed template that's included in the binary itself.
/// </summary>
public class BuiltinTemplate : ProjectTemplate
{
    private Version _engineVersion;
    private Bitmap _icon;
    public Uri? ResourcePath { get; }

    public BuiltinTemplate(string displayName, Uri? resourcePath, Version engineVersion, Bitmap icon)
    {
        Name = displayName;
        ResourcePath = resourcePath;
        _engineVersion = engineVersion;
        _icon = icon;
    }

    #region ProjectTemplate overrides

    /// <inheritdoc/>
    public override int Order { get; } = 0;

    /// <inheritdoc/>
    public override string Name { get; }

    public string ProjectName => string.Concat(Name.Split(' '));

    /// <inheritdoc/>
    public override async void Create(Project newProject)
    {
        var newProjectParentDir = Directory.GetParent(newProject.Path)!.FullName;

        var stream = new FileStream("/home/minebill/Downloads/BasicScene.zip", FileMode.Open, FileAccess.Read);

        // var stream = AssetLoader.Open(path);
        // Console.WriteLine($"Stream has {stream.Length} bytes.\n\tPosition: {stream.Position}");
        // Console.WriteLine($"\tReading 1 byte: {stream.ReadByte()}");
        await ZipHelpers.ExtractToDirectoryAsync(stream, newProjectParentDir, new Progress<float>());

        var unzippedPath = Path.Combine(newProjectParentDir, ProjectName);
        var flaxproj = Path.Combine(unzippedPath, ProjectName) + ".flaxproj";
        var jsonText = await File.ReadAllTextAsync(flaxproj);
        var json = JsonNode.Parse(jsonText);
        if (json != null)
            json["Name"] = newProject.Name;

        await using var writer = new Utf8JsonWriter(new FileStream(flaxproj, FileMode.Create), new JsonWriterOptions
        {
            Indented = true
        });
        json?.WriteTo(writer);

        File.Move(flaxproj, Path.Combine(unzippedPath, newProject.Name + ".flaxproj"));
        Directory.Move(unzippedPath, newProject.Path);

        var projectManager = App.Current.Services.GetService<IProjectManager>();
        projectManager?.AddProject(newProject);
    }

    public override Task<Bitmap> GetIcon()
    {
        return Task.FromResult(_icon);
    }

    /// <inheritdoc/>
    public override Version GetEngineVersion()
    {
        return _engineVersion;
    }

    #endregion
}
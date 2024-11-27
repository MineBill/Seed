using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Launcher.Services;
using NLog;

namespace Launcher.DataModels;

/// <summary>
/// A template bundled inside the launcher binary, in a zip format.
/// </summary>
/// <param name="displayName">The name to use when displaying the template in the UI</param>
/// <param name="actualName">The name of the unzipped project folder AND the project file</param>
/// <param name="description">The description to use when displaying the template in the UI</param>
/// <param name="resourcePath">The URI the zip resource</param>
/// <param name="icon">The icon to use when displaying the template in the UI</param>
public class BundledTemplate(string displayName, string actualName, string description, Uri resourcePath, Bitmap icon)
    : ProjectTemplate
{
    #region ProjectTemplate overrides

    /// <inheritdoc/>
    public override int Order => 0;

    /// <inheritdoc/>
    public override string Name { get; } = displayName;

    private string ProjectName => string.Concat(actualName.Split(' '));

    /// <inheritdoc/>
    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names")]
    public override async Task<Project?> Create(string name, string parentFolder, Engine engine)
    {
        var stream = AssetLoader.Open(resourcePath);
        stream.Seek(0, SeekOrigin.Begin);

        // https://github.com/AvaloniaUI/Avalonia/issues/13604
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        await ZipHelpers.ExtractToDirectoryAsync(ms, parentFolder, new Progress<float>());

        var unzippedPath = Path.Combine(parentFolder, ProjectName);
        var projFile = Path.Combine(unzippedPath, ProjectName) + ".flaxproj";
        var jsonText = await File.ReadAllTextAsync(projFile);
        var json = JsonNode.Parse(jsonText);
        if (json != null)
            json["Name"] = name;

        await using (var file = new FileStream(projFile, FileMode.Create))
        {
            await using var writer = new Utf8JsonWriter(file, new JsonWriterOptions
            {
                Indented = true
            });
            json?.WriteTo(writer);
        }

        var newProjFile = Path.Combine(unzippedPath, name + ".flaxproj");
        File.Move(projFile, newProjFile);
        Directory.Move(unzippedPath, Path.Combine(parentFolder, name));

        return new Project(name, Path.Combine(parentFolder, name), FlaxProj: newProjFile, engine.Version);
    }

    public override Task<Bitmap> Icon => Task.FromResult(icon);

    public override string Description => description;

    /// <inheritdoc/>
    public override EngineVersion? MinimumEngineVersion => null;

    #endregion
}
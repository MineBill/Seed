using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace Seed.Models.ProjectTemplates;

/// <summary>
/// A template that is located on a remote git instance.
/// </summary>
public class GitTemplate : ProjectTemplate
{
    public Uri RemoteUrl { get; }
    public Uri ImagePath { get; }

    public GitTemplate(string name, Uri remoteUrl, Uri imagePath)
    {
        RemoteUrl = remoteUrl;
        ImagePath = imagePath;
        Name = name;
    }

    public override int Order { get; } = 2;

    /// <inheritdoc/>
    public override string Name { get; }

    /// <inheritdoc/>
    public override void Create(Project newProject)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override Task<Bitmap> GetIcon()
    {
        throw new NotImplementedException();
    }

    public override Version GetEngineVersion()
    {
        throw new NotImplementedException();
    }
}
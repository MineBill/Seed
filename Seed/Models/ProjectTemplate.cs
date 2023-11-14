using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace Seed.Models;

/// <summary>
/// Abstract representation of a project template.
/// </summary>
public abstract class ProjectTemplate
{
    /// <summary>
    /// Specifies the order of this type of template in the new project dialog.
    /// Lower values appear on top.
    /// </summary>
    public abstract int Order { get; }

    /// <summary>
    /// The name of this template.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Creates the project.
    /// </summary>
    public abstract void Create(Project newProject);

    /// <summary>
    /// Returns an icon to be used in the new project dialog.
    /// </summary>
    /// <returns></returns>
    public abstract Task<Bitmap> GetIcon();

    /// <summary>
    /// The minimum engine version this template is compatible with.
    /// </summary>
    /// <returns></returns>
    public abstract Version GetEngineVersion();
}
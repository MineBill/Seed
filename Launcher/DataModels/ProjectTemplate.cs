using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace Launcher.DataModels;

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
    /// Returns an icon to be used in the new project dialog.
    /// </summary>
    /// <returns></returns>
    public abstract Task<Bitmap> Icon { get; }

    public abstract string Description { get; }

    /// <summary>
    /// The minimum engine version this template is compatible with.
    /// If null is returned, then this template has no restriction on which engine version can be used with it.
    /// </summary>
    /// <returns></returns>
    public abstract EngineVersion? MinimumEngineVersion { get; }

    /// <summary>
    /// Creates the project.
    /// </summary>
    public abstract Task<Project?> Create(string name, string parentFolder, Engine engine);
}
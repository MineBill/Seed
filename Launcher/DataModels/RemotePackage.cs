using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Launcher.DataModels;

/// <summary>
/// A package is a download available from the flax servers. It can be
/// an Editor or Platform Tools.
/// </summary>
public class RemotePackage(string name, string targetPath, string url)
{
    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = name;

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("requried")] // This is not a typo here, it's a typo in the api response.
    public bool? Required { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("default")]
    public bool? Default { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("targetPath")]
    public string TargetPath { get; set; } = targetPath;

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = url;

    /// <summary>
    /// Returns true if this package is an editor.
    /// </summary>
    public bool IsEditorPackage => Name.Equals("Editor");

    public bool IsLinuxTools => !IsEditorPackage && Name.Contains("Linux");

    public bool IsWindowsTools => !IsEditorPackage && Name.Contains("Windows");

    public bool IsAndroidTools => !IsEditorPackage && Name.Contains("Android");

    /// <summary>
    /// If this is an editor package, it will return the
    /// appropriate url for the current platform. Otherwise,
    /// it'll return <see cref="string.Empty"/>
    /// </summary>
    public string EditorUrl
    {
        get
        {
            var mainPath = Url[..Url.LastIndexOf('/')];

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Url;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return $"{mainPath}/FlaxEditorLinux.zip";
            }

            // TODO: Maybe remove this too, can't test this anyway.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return $"{mainPath}/FlaxEditor.dmg";
            }

            return string.Empty;
        }
    }
}

[JsonSerializable(typeof(RemotePackage))]
internal partial class RemotePackageGenerationContext : JsonSerializerContext;
using Launcher.DataModels;

namespace Launcher.Services;

public interface IPreferencesManager
{
    public UserPreferences Preferences { get; }

    public void Save();

    /// <summary>
    /// Returns the engine installation location from the user preferences if it is set,
    /// otherwise the default location is returned.
    /// </summary>
    /// <returns></returns>
    public string GetInstallLocation() =>
        Preferences.EngineInstallLocation ?? Globals.GetDefaultEngineInstallLocation();
}
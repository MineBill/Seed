using Seed.Models;

namespace Seed.Services;

/// <summary>
/// Interface to save and load user preferences.
/// Can be used to save in other formats, like YAML, if we ever hate out users.
/// </summary>
public interface IPreferencesSaver
{
    public UserPreferences Preferences { get; }

    public void Save();
}
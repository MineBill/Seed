using Launcher.DataModels;

namespace Launcher.Services;

public interface IPreferencesManager
{
    public UserPreferences Preferences { get; }

    public void Save();
}
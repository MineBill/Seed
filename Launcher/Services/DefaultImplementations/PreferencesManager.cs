using System;
using System.IO;
using System.Text.Json;
using Launcher.DataModels;
using NLog;

namespace Launcher.Services.DefaultImplementations;

public class JsonPreferencesManager : IPreferencesManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        TypeInfoResolver = UserPreferencesGenerationContext.Default
    };

    public UserPreferences Preferences { get; } = Load();

    public void Save()
    {
        var path = Globals.GetPreferencesFileLocation();
        using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        JsonSerializer.Serialize(file, Preferences, SerializerOptions);
        file.Close();
    }

    private static UserPreferences Load()
    {
        var path = Globals.GetPreferencesFileLocation();
        if (!File.Exists(path))
            return new UserPreferences();
        try
        {
            // TODO: Handle exceptions about permissions, etc.
            var jsonStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var preferences = JsonSerializer.Deserialize<UserPreferences>(jsonStream, SerializerOptions);
            jsonStream.Close();
            return preferences ?? new UserPreferences();
        }
        catch (Exception e)
        {
            Logger.Error(e, "Exception caught while attempting to deserialize user settings.");
            return new UserPreferences();
        }
    }
}
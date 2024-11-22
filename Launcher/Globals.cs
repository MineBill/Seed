using System;
using System.IO;

namespace Launcher;

public static class Globals
{
    public static readonly Uri RepoUrl = new Uri("https://github.com/MineBill/Seed");
    public const string AppName = "SeedLauncher";
    public const string EnginesSaveFileName = "Engines.json";
    public const string ProjectsSaveFileName = "Projects.json";
    public const string UserPreferencesSaveFileName = "Preferences.json";
    public const string LogFileName = "Log.txt";

    public static string GetConfigFolder()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
    }

    public static string GetDefaultEngineInstallLocation()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, AppName, "Versions");
    }

    public static string GetDefaultProjectLocation()
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        return Path.Combine(documents, "FlaxProjects");
    }

    public static string GetPreferencesFileLocation()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, AppName, UserPreferencesSaveFileName);
    }

    public static string GetLogFileLocation()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, AppName, LogFileName);
    }
}
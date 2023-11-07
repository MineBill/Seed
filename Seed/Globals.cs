using System;
using System.IO;

namespace Seed;

public static class Globals
{
    public const string AppName = "SeedLauncher";
    public const string EnginesSaveFileName = "Engines.json";
    public const string ProjectsSaveFileName = "Projects.json";

    public static string GetDefaultEngineInstallLocation()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, AppName, "Versions");
    }

    public static string GetDefaultProjectLocation()
    {
        return "";
    }
}
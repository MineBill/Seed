using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Seed.Models;

namespace Seed.Services;

public class ProjectDiscoveryService
{
    public const string AppName = "SeedLauncher";
    public const string ProjectsSaveFile = "Projects.json";

    public ProjectInfo? Load()
    {
        var configFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
        var saveFile = Path.Combine(configFolder, ProjectsSaveFile);
        if (!File.Exists(saveFile))
            return null;
        
        var json = File.ReadAllText(saveFile);
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            // TODO: Handle this properly
            var projectInfo = JsonSerializer.Deserialize<ProjectInfo>(json)!;
            return projectInfo;
        }
        catch (JsonException je)
        {
            Console.WriteLine($"Exception while attempting to deserialize project info: {je}");
            return null;
        }
    }

    public void Save(ProjectInfo info)
    {
        var configFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
        if (!Directory.Exists(configFolder))
            Directory.CreateDirectory(configFolder);

        var json = JsonSerializer.Serialize(info);
        var saveFile = Path.Combine(configFolder, ProjectsSaveFile);
        if (!File.Exists(saveFile))
        {
            var stream = File.Create(saveFile);
            stream.Close();
        }
        
        File.WriteAllText(saveFile, json);
    }
}
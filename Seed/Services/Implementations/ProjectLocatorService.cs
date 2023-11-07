using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Seed.Models;

namespace Seed.Services;

public class ProjectLocatorService: IProjectLocatorService
{
    public const string AppName = "SeedLauncher";
    public const string ProjectsSaveFile = "Projects.json";
    
    private ProjectInfo _projectInfo = new();
    
    public ProjectLocatorService()
    {
        Load();
    }

    private void Load()
    {
        var configFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
        var saveFile = Path.Combine(configFolder, ProjectsSaveFile);
        if (!File.Exists(saveFile))
            return;
        
        var json = File.ReadAllText(saveFile);
        if (string.IsNullOrWhiteSpace(json))
            return;
        try
        {
            // TODO: Handle this properly
            var projectInfo = JsonSerializer.Deserialize<ProjectInfo>(json);
            if (projectInfo is null)
            {
                // TODO: Log this
                return;
            }
            _projectInfo = projectInfo;
        }
        catch (JsonException je)
        {
            Console.WriteLine($"Exception while attempting to deserialize project info: {je}");
            return;
        }
    }
    
    public List<Project> GetProjects()
    {
        return _projectInfo.Projects;
    }
}
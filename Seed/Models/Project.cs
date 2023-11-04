namespace Seed.Models;

public class Project
{
    public string Name { get; set; } = string.Empty;
    public EngineVersion Version { get; set; } = new(0, 0, 0, 0);
    public string IconPath { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public Project(string name, EngineVersion version, string iconPath)
    {
        Name = name;
        Version = version;
        IconPath = iconPath;
    }

    public Project()
    {
        
    }
}
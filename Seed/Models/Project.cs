using System;

namespace Seed.Models;

public record Project(string Name, string Path, Version? Version = null)
{
    public string Name { get; set; } = Name;
    public string Path { get; set; } = Path;
    public Version? Version { get; set; } = Version;
    
    public string IconPath => System.IO.Path.Combine(Path, "Cache", "icon.png");
}
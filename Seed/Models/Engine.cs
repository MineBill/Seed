using System;

namespace Seed.Models;

public class Engine
{
    public string Path { get; set; } = string.Empty;
    public Version Version { get; set; } = new Version();
}
namespace Seed.Models;

public record EngineVersion(int Major, int Minor, int Build, int Revision)
{
    public int Major { get; set; } = Major;
    public int Minor { get; set; } = Minor;
    public int Build { get; set; } = Build;
    public int Revision { get; set; } = Revision;

    public override string ToString()
    {
        return Revision == 0 ? $"{Major}.{Minor}" : $"{Major}.{Minor}.{Revision}";
    }
}
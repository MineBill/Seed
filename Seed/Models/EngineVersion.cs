using System;
using System.Text.Json.Serialization;

namespace Seed.Models;

[JsonDerivedType(typeof(NormalVersion), typeDiscriminator: "normal")]
[JsonDerivedType(typeof(GitVersion), typeDiscriminator: "git")]
[JsonDerivedType(typeof(LocalBuild), typeDiscriminator: "local")]
public abstract record EngineVersion : IComparable<EngineVersion>
{
    public abstract int CompareTo(EngineVersion? other);
}

public record NormalVersion(Version Version) : EngineVersion
{
    public override int CompareTo(EngineVersion? other)
    {
        if (other is NormalVersion otherVersion)
            return Version.CompareTo(otherVersion.Version);
        return 0;
    }

    public override string ToString()
    {
        return $"{Version.Major}.{Version.Minor}" + (Version.Revision != -1 ? $".{Version.Revision}" : string.Empty);
    }
}

public record GitVersion(string Commit, DateTime CreationTime) : EngineVersion
{
    public override int CompareTo(EngineVersion? other)
    {
        if (other is GitVersion gitVersion)
            return string.Compare(Commit, gitVersion.Commit, StringComparison.Ordinal);
        return 0;
    }

    public override string ToString()
    {
        return Commit;
    }
}

public record LocalBuild(string Path, Version Version) : EngineVersion
{
    public override int CompareTo(EngineVersion? other)
    {
        if (other is NormalVersion normal)
        {
            return Version.CompareTo(normal.Version);
        }
        return 0;
    }

    public override string ToString()
    {
        var version = $"{Version.Major}.{Version.Minor}.{Version.Revision}";
        return Path + " " + version;
    }
}
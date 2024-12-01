using System;
using System.Diagnostics.CodeAnalysis;

namespace Launcher;

/// <summary>
/// Represent an "empty" type like void, that can be used as a generic type.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Unit other && Equals(other);
    }

    public override int GetHashCode() => 0;

    public override string ToString() => "()";

    public static bool operator ==(Unit left, Unit right) => true;

    public static bool operator !=(Unit left, Unit right)
    {
        return !(left == right);
    }

    public bool Equals(Unit other) => true;
}
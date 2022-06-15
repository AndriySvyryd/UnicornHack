using System;

namespace UnicornHack.Utils.DataStructures;

public readonly struct Dimensions : IEquatable<Dimensions>
{
    public Dimensions(byte width, byte height)
    {
        Width = width;
        Height = height;
    }

    public byte Width
    {
        get;
        init;
    }

    public byte Height
    {
        get;
        init;
    }

    public bool Contains(Dimensions d) => Width >= d.Width && Height >= d.Height;

    public bool Equals(Dimensions other)
        => Width == other.Width && Height == other.Height;

    public override bool Equals(object obj)
        => obj is Dimensions other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Width, Height);

    public static bool operator ==(Dimensions left, Dimensions right)
        => left.Equals(right);

    public static bool operator !=(Dimensions left, Dimensions right)
        => !left.Equals(right);
}

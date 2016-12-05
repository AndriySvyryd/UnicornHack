using System;

namespace UnicornHack
{
    [Flags]
    public enum GenerationFlags
    {
        None,
        NonGenocidable = 1 << 1,
        NonPolymorphable = 1 << 2,
        SmallGroup = 1 << 3,
        LargeGroup = 1 << 4,
        Entourage = 1 << 5,
        HellOnly = 1 << 6,
        NoHell = 1 << 7
    }
}
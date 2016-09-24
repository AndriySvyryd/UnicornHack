using System;

namespace UnicornHack.Models.GameDefinitions
{
    [Flags]
    public enum SpeciesClass : ulong
    {
        None = 0,
        Vermin = 1 << 0,
        Canine = 1 << 1,
        Feline = 1 << 2,
        Quadrupedal = 1 << 3,
        Rodent = 1 << 4,
        Bird = 1 << 5,
        Reptile = 1 << 6,
        Demon = 1 << 7,
        Celestial = 1 << 8,
        Fey = 1 << 9,
        ShapeChanger = 1 << 10,
        MagicalBeast = 1 << 11,
        Aberration = 1 << 12,
        Extraplanar = 1 << 13,
        Undead = 1 << 14
    }
}
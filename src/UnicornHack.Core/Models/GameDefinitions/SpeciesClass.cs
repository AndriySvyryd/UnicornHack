using System;

namespace UnicornHack.Models.GameDefinitions
{
    [Flags]
    public enum SpeciesClass : ulong
    {
        None = 0,
        Insect,
        Canine,
        Feline,
        Equine,
        Rodent,
        Bird,
        Demon,
        DivineBeing,
        Fey,
        ShapeChanger,
        Hybrid,
        Undead
    }
}
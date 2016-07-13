using System;

namespace UnicornHack.Models.GameDefinitions
{
    [Flags]
    public enum MonsterWarning
    {
        None,
        All,
        Danger,
        Undead,
        Orc,
        Elf,
        Human
    }
}
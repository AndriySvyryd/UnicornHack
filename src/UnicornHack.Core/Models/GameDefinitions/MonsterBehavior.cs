using System;

namespace UnicornHack.Models.GameDefinitions
{
    [Flags]
    public enum MonsterBehavior
    {
        None = 0,
        AlignmentAware = 1 << 0,
        Peaceful = 1 << 1,
        RangedPeaceful = 1 << 2,
        Domesticable = 1 << 3,
        Mountable = 1 << 4,
        Wandering = 1 << 5,
        Stalking = 1 << 6,
        Displacing = 1 << 7,
        GoldCollector = 1 << 8,
        GemCollector = 1 << 9,
        WeaponCollector = 1 << 10,
        MagicUser = 1 << 11,
        Covetous = 1 << 12,
        Bribeable = 1 << 13
    }
}
using System;

namespace UnicornHack.Primitives
{
    [Flags]
    public enum EquipmentSlot
    {
        None = 0,
        GraspPrimaryMelee = 1 << 0,
        GraspSecondaryMelee = 1 << 1,
        GraspBothMelee = 1 << 2,
        GraspPrimaryRanged = 1 << 3,
        GraspSecondaryRanged = 1 << 4,
        GraspBothRanged = 1 << 5,
        GraspMouth = 1 << 6,
        Torso = 1 << 7,
        Head = 1 << 8,
        Feet = 1 << 9,
        Hands = 1 << 10,
        Back = 1 << 11,
        Neck = 1 << 12,

        GraspSingleMelee = GraspPrimaryMelee | GraspSecondaryMelee,
        GraspSingleRanged = GraspPrimaryRanged | GraspSecondaryRanged,
        GraspMelee = GraspSingleMelee | GraspBothMelee,
        GraspRanged = GraspSingleRanged | GraspBothRanged
    }
}

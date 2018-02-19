using System;

namespace UnicornHack
{
    [Flags]
    public enum EquipmentSlot
    {
        Default = 0,
        GraspPrimaryExtremity = 1 << 0,
        GraspSecondaryExtremity = 1 << 1,
        GraspSingleExtremity = GraspPrimaryExtremity | GraspSecondaryExtremity,
        GraspBothExtremities = 1 << 2,
        GraspMouth = 1 << 3,
        Feet = 1 << 4,
        Back = 1 << 5,
        Torso = 1 << 6,
        Head = 1 << 7,
        Hands = 1 << 8,
        Neck = 1 << 9
    }
}
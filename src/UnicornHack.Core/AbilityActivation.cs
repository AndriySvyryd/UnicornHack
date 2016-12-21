using System;

namespace UnicornHack
{
    [Flags]
    public enum AbilityActivation
    {
        Default = 0,
        Always = 1 << 0,
        WhileActivated = 1 << 1,
        WhileEquipped = 1 << 2,
        OnActivation = 1 << 3,
        OnTarget = 1 << 4,
        OnMeleeAttack = 1 << 5,
        OnRangedAttack = 1 << 6,
        OnSpellCast = 1 << 7,
        OnMeleeHit = 1 << 8,
        OnRangedHit = 1 << 9,
        OnSpellHit = 1 << 10,
        OnDigestion = 1 << 11,
        OnConsumption = 1 << 12,
        OnDeath = 1 << 13,
        OnPickup = 1 << 14
    }
}
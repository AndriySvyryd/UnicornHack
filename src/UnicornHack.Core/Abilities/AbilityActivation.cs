using System;

namespace UnicornHack.Abilities
{
    [Flags]
    public enum AbilityActivation
    {
        Default = 0,
        Always = 1 << 0,
        WhileToggled = 1 << 1,
        WhilePossessed = 1 << 2,
        WhileEquipped = 1 << 3,
        OnActivation = 1 << 4,
        OnTarget = 1 << 5,
        OnMeleeAttack = 1 << 6,
        OnRangedAttack = 1 << 7,
        OnSpellCast = 1 << 8,
        OnMeleeHit = 1 << 9,
        OnRangedHit = 1 << 10,
        OnSpellHit = 1 << 11,
        OnDigestion = 1 << 12,
        OnConsumption = 1 << 13,
        OnDeath = 1 << 14,
        OnTimeout = 1 << 15,
        OnLevelUp = 1 << 16
    }
}
using System;

namespace UnicornHack.Models.GameDefinitions
{
    [Flags]
    public enum AbilityActivation
    {
        Dafault = 0,
        Manual = 1 << 0,
        Targetted = 1 << 1,
        Sustained = 1 << 2,
        OnMeleeAttack = 1 << 3,
        OnRangedAttack = 1 << 4,
        OnSpellCast = 1 << 5,
        OnMeleeHit = 1 << 6,
        OnRangedHit = 1 << 7,
        OnSpellHit = 1 << 8,
        OnDigestion = 1 << 9,
        OnConsumption = 1 << 10,
        OnDeath = 1 << 11
    }
}
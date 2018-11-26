using System;

namespace UnicornHack.Primitives
{
    [Flags]
    public enum ActivationType
    {
        Default = 0,
        Always = 1 << 0,
        WhileToggled = 1 << 1,
        WhilePossessed = 1 << 2,
        WhileEquipped = 1 << 3,
        Continuous = Always | WhileToggled | WhilePossessed | WhileEquipped,
        ManualActivation = 1 << 4,
        Targeted = 1 << 5,
        Slottable = WhileToggled | ManualActivation | Targeted,
        OnPhysicalMeleeAttack = 1 << 6,
        OnPhysicalRangedAttack = 1 << 7,
        OnPhysicalAttack = OnPhysicalMeleeAttack | OnPhysicalRangedAttack,
        OnMagicalMeleeAttack = 1 << 8,
        OnMagicalRangedAttack = 1 << 9,
        OnMagicalAttack = OnMagicalMeleeAttack | OnMagicalRangedAttack,
        OnMeleeAttack = OnPhysicalMeleeAttack | OnMagicalMeleeAttack,
        OnRangedAttack = OnPhysicalRangedAttack | OnMagicalRangedAttack,
        OnAttack = OnMeleeAttack | OnRangedAttack,
        OnPhysicalMeleeHit = 1 << 10,
        OnPhysicalRangedHit = 1 << 11,
        OnPhysicalHit = OnPhysicalMeleeHit | OnPhysicalRangedHit,
        OnMagicalMeleeHit = 1 << 12,
        OnMagicalRangedHit = 1 << 13,
        OnMagicalHit = OnMagicalMeleeHit | OnMagicalRangedHit,
        OnMeleeHit = OnPhysicalMeleeHit | OnMagicalMeleeHit,
        OnRangedHit = OnPhysicalRangedHit | OnMagicalRangedHit,
        OnHit = OnMeleeHit | OnRangedHit,
        OnDeath = 1 << 14,
        OnLevelUp = 1 << 15,
        OnDigestion = 1 << 16
    }
}

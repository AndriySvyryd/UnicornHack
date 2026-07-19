namespace UnicornHack.Primitives;

/// <summary>
///     Flags describing how an ability becomes active or which event triggers it. Primitive flags may be
///     combined; the composite members group modes used by activation, slotting, and trigger consumers.
/// </summary>
[Flags]
public enum ActivationType
{
    /// <summary>No activation mode or trigger is specified.</summary>
    Default = 0,

    /// <summary>Activates automatically while its owner carries the ability.</summary>
    Always = 1 << 0,

    /// <summary>Can be toggled on and remains active until toggled off.</summary>
    WhileToggled = 1 << 1,

    /// <summary>Remains active while the owning item is possessed by an affectable being.</summary>
    WhilePossessed = 1 << 2,

    /// <summary>Remains active while the owning item is equipped.</summary>
    WhileEquipped = 1 << 3,

    /// <summary>Is explicitly activated without selecting a target.</summary>
    Manual = 1 << 5,

    /// <summary>Is explicitly activated against a selected entity or map cell.</summary>
    Targeted = 1 << 6,

    /// <summary>Triggers when its owner performs a melee attack, whether or not the attack hits.</summary>
    OnMeleeAttack = 1 << 7,

    /// <summary>Triggers when its owner performs a ranged attack, whether or not the attack hits.</summary>
    OnRangedAttack = 1 << 8,

    /// <summary>Triggers when its owner's melee attack hits.</summary>
    OnMeleeHit = 1 << 10,

    /// <summary>Triggers when its owner's ranged attack hits.</summary>
    OnRangedHit = 1 << 11,

    /// <summary>Triggers when its owner dies.</summary>
    OnDeath = 1 << 14,

    /// <summary>All modes whose effects remain active continuously while their condition holds.</summary>
    Continuous = Always | WhileToggled | WhilePossessed | WhileEquipped,

    /// <summary>All modes that can occupy and be invoked through an ability slot.</summary>
    Slottable = WhileToggled | Manual | Targeted,

    /// <summary>All triggers that fire when an attack is attempted.</summary>
    OnAttack = OnMeleeAttack | OnRangedAttack,

    /// <summary>All triggers that fire only when an attack hits.</summary>
    OnHit = OnMeleeHit | OnRangedHit
}

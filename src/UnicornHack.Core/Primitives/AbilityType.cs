namespace UnicornHack.Primitives;

/// <summary>
///     The category of an ability, which governs how it is acquired, how (or whether) its level is raised, and
///     how it is grouped and surfaced to the player. Every ability carries exactly one type.
/// </summary>
public enum AbilityType
{
    /// <summary>
    ///     An ability with no special classification. It is activated and surfaced on its own
    ///     rather than as part of a skill, trait, feat, item or status grouping.
    /// </summary>
    Default = 0,

    /// <summary>The built-in melee attack occupying the default melee slot.</summary>
    DefaultMeleeAttack = 1,

    /// <summary>The built-in ranged attack occupying the default ranged slot.</summary>
    DefaultRangedAttack = 2,

    /// <summary>An ordinary melee attack.</summary>
    MeleeAttack = 3,

    /// <summary>An ordinary ranged attack.</summary>
    RangedAttack = 4,

    /// <summary>A spell delivered through melee contact.</summary>
    MeleeSpell = 5,

    /// <summary>A spell delivered at range.</summary>
    RangedSpell = 6,

    /// <summary>A divine ability granted by faith.</summary>
    Miracle = 7,

    /// <summary>
    ///     The being's built-in derived-stat carrier. Its property contributions establish the base layer
    ///     before race, equipment, status and other ability modifiers.
    /// </summary>
    Innate = 8,

    /// <summary>
    ///     A species or ancestry carrier whose cumulative effects define the owner's innate body and abilities.
    /// </summary>
    Race = 9,

    /// <summary>
    ///     An ability provided by an item - typically its activation (drink, read, zap, ...) or an effect it
    ///     grants while equipped - and available only for as long as the item is carried or worn.
    /// </summary>
    Item = 10,

    /// <summary>
    ///     A status condition (e.g. stunned, blinded, on fire) carried as an ability on the affected being
    ///     while the condition is in force; its contained effects are the condition's property riders, and
    ///     its presence is what game logic and the client read instead of a bespoke calculated bool.
    /// </summary>
    Status = 11,

    /// <summary>A temporary beneficial condition.</summary>
    Buff = 12,

    /// <summary>A beneficial condition that is generally longer-lived than a buff.</summary>
    Boon = 13,

    /// <summary>A harmful condition that is generally longer-lived than a status.</summary>
    Curse = 14,

    /// <summary>
    ///     A character trait, usually of a physical nature.
    /// </summary>
    Trait = 15,

    /// <summary>A negative, usually innate characteristic.</summary>
    Flaw = 16,

    /// <summary>
    ///     A character feat, usually of a behavioral nature.
    /// </summary>
    Feat = 17,

    /// <summary>A negative, usually acquired characteristic.</summary>
    Fault = 18,

    /// <summary>
    ///     A learnable skill, providing abilities and bonuses.
    /// </summary>
    Skill = 19,

    /// <summary>A character's devotion to a deity, which grants and advances miracles.</summary>
    Faith = 20
}

public static class AbilityTypeExtensions
{
    public static bool IsDefaultAttack(this AbilityType type)
        => type is AbilityType.DefaultMeleeAttack or AbilityType.DefaultRangedAttack;

    public static bool IsMeleeAttack(this AbilityType type)
        => type is AbilityType.DefaultMeleeAttack or AbilityType.MeleeAttack or AbilityType.MeleeSpell;

    public static bool IsRangedAttack(this AbilityType type)
        => type is AbilityType.DefaultRangedAttack or AbilityType.RangedAttack or AbilityType.RangedSpell;

    public static bool IsAttack(this AbilityType type)
        => type.IsMeleeAttack() || type.IsRangedAttack();

    public static bool IsSpell(this AbilityType type)
        => type is AbilityType.MeleeSpell or AbilityType.RangedSpell;

    public static bool IsStatusLike(this AbilityType type)
        => type is AbilityType.Status or AbilityType.Buff or AbilityType.Boon or AbilityType.Curse;

    public static bool IsTraitLike(this AbilityType type)
        => type is AbilityType.Trait or AbilityType.Flaw;

    public static bool IsFeatLike(this AbilityType type)
        => type is AbilityType.Feat or AbilityType.Fault;

    public static bool IsAdaptation(this AbilityType type)
        => type.IsTraitLike() || type.IsFeatLike();

    public static bool IsGrantable(this AbilityType type)
        => type == AbilityType.Skill || type.IsAdaptation();
}

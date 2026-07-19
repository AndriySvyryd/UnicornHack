namespace UnicornHack.Systems.Effects;

/// <summary>
///     A persisted directive recording a structural effect (an added component carried by a separate
///     entity) applied to a target. Stored inline in the target's <see cref="ActiveEffectsComponent" />.
/// </summary>
public class ActiveComponentEffect : ActiveEffect
{
    private int _sourceAbilityEntityId;
    private int _carrierId;
    private int? _conditionValue;
    private int _level;

    public ActiveComponentEffect(
        int gameId, int entityId, int instanceId, EffectType effectType, string? targetName)
        : base(gameId, entityId, instanceId, effectType, targetName)
    {
    }

    /// <summary>The ability entity that granted this component (the source/containing ability).</summary>
    public int SourceAbilityEntityId
    {
        get => _sourceAbilityEntityId;
        set => SetWithNotify(value, ref _sourceAbilityEntityId);
    }

    /// <summary>
    ///     The separate entity that holds the added component, or zero while the contributed range is
    ///     suppressed or the record is being attached or detached.
    /// </summary>
    public int CarrierId
    {
        get => _carrierId;
        set => SetWithNotify(value, ref _carrierId);
    }

    /// <summary>
    ///     Effect-specific condition data. A type-wide <see cref="EffectType.DecreaseAbilityLevel" /> stores
    ///     its target <see cref="AbilityType" /> value here.
    /// </summary>
    public int? ConditionValue
    {
        get => _conditionValue;
        set => SetWithNotify(value, ref _conditionValue);
    }

    /// <summary>This source's contribution to the added component's level (summed across sources).</summary>
    public int Level
    {
        get => _level;
        set => SetWithNotify(value, ref _level);
    }

}

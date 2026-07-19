namespace UnicornHack.Systems.Effects;

/// <summary>
///     A single lingering modifier effect currently active on an affectable entity. Stored inline in
///     the owner's <see cref="ActiveEffectsComponent" /> (an EF owned collection), never as its own
///     entity. Magnitudes are cached: static effects resolve them once at apply time; dynamic effects
///     recompute them when one of the inputs their definition reads changes.
/// </summary>
public class ActivePropertyEffect : ActiveEffect
{
    private ValueCombinationFunction _combinationFunction;
    private int? _sourceAbilityEntityId;
    private AbilityType _sourceAbilityType;
    private ActivationType _sourceAbilityActivation;
    private int _magnitudePrimary;
    private int _magnitudeSecondary;

    public ActivePropertyEffect(
        int gameId, int entityId, int instanceId, EffectType effectType, string? targetName)
        : base(gameId, entityId, instanceId, effectType, targetName)
    {
        Metadata = effectType == EffectType.Default ? null : Effect.GetMetadata(effectType);
    }

    public EffectMetadata? Metadata
    {
        get;
        private set;
    }

    /// <summary>The combination behavior for this instance, overriding its effect type's metadata.</summary>
    public ValueCombinationFunction CombinationFunction
    {
        get => _combinationFunction;
        set => SetWithNotify(value, ref _combinationFunction);
    }

    public int? SourceAbilityEntityId
    {
        get => _sourceAbilityEntityId;
        set => SetWithNotify(value, ref _sourceAbilityEntityId);
    }

    /// <summary>Cached from the source ability.</summary>
    public AbilityType SourceAbilityType
    {
        get => _sourceAbilityType;
        set => SetWithNotify(value, ref _sourceAbilityType);
    }

    /// <summary>Cached from the source ability so the reducer can order contributions without dereferencing.</summary>
    public ActivationType SourceAbilityActivation
    {
        get => _sourceAbilityActivation;
        set => SetWithNotify(value, ref _sourceAbilityActivation);
    }

    /// <summary>
    ///     The instance's primary magnitude; its meaning depends on <see cref="EffectType" />. For a
    ///     ChangeProperty modifier it is the amount contributed to <see cref="TargetName" />. For a
    ///     status effect it is the value bound onto the condition property (e.g. <c>1</c> for a boolean
    ///     condition, or the visibility bump a burn applies). For a resource-over-time or continuous build-up
    ///     source it is the per-action rate.
    /// </summary>
    public int MagnitudePrimary
    {
        get => _magnitudePrimary;
        set => SetWithNotify(value, ref _magnitudePrimary);
    }

    /// <summary>
    ///     The instance's secondary magnitude, used when one value is not enough. For a polymorph identity
    ///     marker it holds the assumed form's id.
    /// </summary>
    public int MagnitudeSecondary
    {
        get => _magnitudeSecondary;
        set => SetWithNotify(value, ref _magnitudeSecondary);
    }
}

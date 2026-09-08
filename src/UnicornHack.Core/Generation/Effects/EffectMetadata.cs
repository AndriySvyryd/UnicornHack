using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

/// <summary>
///     The distinct on-hit status a damage school inflicts and how it scales: the status effect type, the
///     build-up points per 10 damage and its fixed strength.
/// </summary>
public readonly struct InflictedStatus
{
    public readonly EffectType Status;
    public readonly int AmountFactor;
    public readonly int Amount;
    public readonly bool Clears;

    public InflictedStatus(EffectType status, int amountFactor, int amount, bool clears = false)
    {
        Status = status;
        AmountFactor = amountFactor;
        Amount = amount;
        Clears = clears;
    }

    public static InflictedStatus Clear(EffectType status)
        => new(status, 0, 0, clears: true);
}

public delegate EffectResultMessage? EffectResultMessageSelector(
    EffectComponent effect, GameEntity activatorEntity, GameManager manager);

public delegate void BuildUpRealizationHandler(in BuildUpRealizationContext context);

public readonly struct BuildUpRealizationContext(
    EffectApplicationSystem system,
    EffectAccumulator accumulator,
    GameEntity actorEntity,
    GameManager manager,
    int intervals)
{
    public EffectApplicationSystem System { get; } = system;
    public EffectAccumulator Accumulator { get; } = accumulator;
    public GameEntity ActorEntity { get; } = actorEntity;
    public GameManager Manager { get; } = manager;
    public int Intervals { get; } = intervals;
}

/// <summary>
///     The single source of truth for every per-<see cref="EffectType" /> behavioral fact the runtime keys
///     off. Each concrete <see cref="Effect" /> registers its own entry from a module initializer (see
///     <see cref="Effect.RegisterMetadata" />), so adding an effect means adding one metadata line in that
///     effect's own file rather than editing parallel switch tables.
/// </summary>
public sealed class EffectMetadata
{
    private Func<GameEntity, GameEntity, GameEntity?, int, int?>? _statusAbilityDurationFunction;

    /// <summary>The effect type this metadata describes; its key in the registry.</summary>
    public EffectType EffectType { get; init; }

    /// <summary>The stateless callback that applies an instance of this effect type.</summary>
    public EffectApplicationHandler ApplicationHandler { get; init; } = null!;

    /// <summary>Creates and populates the specialized pooled result message an effect requires.</summary>
    public EffectResultMessageSelector? ResultMessageSelector { get; init; }

    /// <summary>Evolves this build-up and applies any effect-specific consequence at an actor's turn start.</summary>
    public BuildUpRealizationHandler? BuildUpRealizationHandler { get; init; }

    /// <summary>The non-calculated <see cref="BeingComponent" /> resource directly changed by the amount.</summary>
    public string? ResourceProperty { get; init; }

    /// <summary>Whether resource removed from the target is restored to the effect activator.</summary>
    public bool RestoresActivator { get; init; }

    /// <summary>Whether each successful movement extends this lingering effect by one action interval.</summary>
    public bool ExtendsOnMovement { get; init; }

    /// <summary>The calculated resistance property reducing the effect amount, or <c>null</c> if unresisted.</summary>
    public string? ResistanceProperty { get; init; }

    /// <summary>The category of the status effect.</summary>
    public StatusCategory? StatusCategory { get; init; }

    /// <summary>The calculated absorption property reducing damage before resistance (physical armor), or <c>null</c>.</summary>
    public string? AbsorptionProperty { get; init; }

    /// <summary>Whether <see cref="BeingComponent.MagicResistance" /> also reduces this effect.</summary>
    public bool IncludesMagicResistance { get; init; }

    /// <summary>Whether the effect still applies when the containing ability misses or otherwise fails.</summary>
    public bool AppliesOnFailure { get; init; }

    /// <summary>
    ///     The status ability the effect imposes while in force, or <c>null</c>. For a periodic resource
    ///     effect this is the marker ability it holds while active (burning/blight/wither).
    /// </summary>
    public string? StatusAbilityName { get; init; }

    /// <summary>The duration expression for the runtime AddAbility effect carrying this status.</summary>
    public string? StatusAbilityDuration { get; init; }

    /// <summary>How a re-application combines with an active instance of the same type (the stacking axis).</summary>
    public DurationPolicy DurationPolicy { get; init; } = DurationPolicy.Refresh;

    /// <summary>The magnitude combination function a build-up of this type banks with.</summary>
    public ValueCombinationFunction CombinationFunction { get; init; } = ValueCombinationFunction.Sum;

    /// <summary>Whether live instance magnitudes are summed into the status ability's property effects.</summary>
    public bool PropagatesAmountToStatusAbility { get; init; }

    /// <summary>The duration expression used when an effect instance does not override it.</summary>
    public string? DefaultDurationAmount { get; init; }

    /// <summary>The duration used when an effect instance does not override it.</summary>
    public EffectDuration Duration { get; init; }

    /// <summary>The amount expression used when an effect instance does not override it (damage, heal, build-up, ...).</summary>
    public string? DefaultAmount { get; init; }

    /// <summary>The status applications and removals this effect inflicts after changing its resource.</summary>
    public IReadOnlyList<InflictedStatus>? Inflicts { get; init; }

    public int? GetStatusAbilityDuration(GameEntity targetEntity, int level)
    {
        var expression = StatusAbilityDuration
                         ?? throw new InvalidOperationException(
                             $"EffectType.{EffectType} has no status ability duration.");
        _statusAbilityDurationFunction ??=
            EffectApplicationSystem.CreateStatusAbilityDurationFunction(
                expression, StatusAbilityName ?? "status ability", EffectType.ToString());
        return EffectApplicationSystem.InvokeStatusAbilityDurationFunction(
            _statusAbilityDurationFunction, targetEntity, level,
            StatusAbilityName ?? EffectType.ToString());
    }

    private static void RealizeStatusBuildUp(in BuildUpRealizationContext context)
    {
        var metadata = Effect.GetMetadata(context.Accumulator.EffectType);
        var statusLevel = metadata.StatusAbilityName is { } abilityName
            ? EffectApplicationSystem.GetAbilityLevel(context.ActorEntity, abilityName)
            : 0;
        context.System.RealizeBuildUp(context, decay: 10 * (statusLevel + 1));
    }

    public static EffectMetadata Default(
        EffectType effectType,
        string? abilityName = null,
        EffectDuration defaultDuration = EffectDuration.Instant,
        string? defaultDurationAmount = null,
        string? statusAbilityDuration = null,
        DurationPolicy durationPolicy = DurationPolicy.Refresh,
        ValueCombinationFunction combinationFunction = ValueCombinationFunction.BuildUpFixed,
        string? defaultAmount = null,
        EffectApplicationHandler? applicationHandler = null,
        EffectResultMessageSelector? resultMessageSelector = null,
        BuildUpRealizationHandler? buildUpRealizationHandler = null,
        string? resistanceProperty = null,
        bool appliesOnFailure = false,
        IReadOnlyList<InflictedStatus>? inflicts = null,
        bool propagatesAmountToStatusAbility = false,
        StatusCategory? statusCategory = null)
        => Effect.RegisterMetadata(new EffectMetadata
        {
            EffectType = effectType,
            ApplicationHandler = applicationHandler ?? (static (in context) => context.System.ApplyStatus(context)),
            ResultMessageSelector = resultMessageSelector,
            BuildUpRealizationHandler = buildUpRealizationHandler
                                        ?? (combinationFunction.IsBuildUp()
                                            ? RealizeStatusBuildUp
                                            : null),
            ResistanceProperty = resistanceProperty,
            StatusCategory = statusCategory,
            StatusAbilityName = abilityName,
            StatusAbilityDuration = statusAbilityDuration,
            CombinationFunction = combinationFunction,
            PropagatesAmountToStatusAbility = propagatesAmountToStatusAbility,
            Duration = defaultDuration,
            DefaultDurationAmount = defaultDurationAmount,
            DurationPolicy = durationPolicy,
            DefaultAmount = defaultAmount,
            AppliesOnFailure = appliesOnFailure,
            Inflicts = inflicts
        });

    public static EffectMetadata Resource(
        EffectType effectType, string? resistanceProperty = null,
        string resourceProperty = nameof(BeingComponent.HitPoints),
        string? absorptionProperty = null, bool includesMagicResistance = true,
        IReadOnlyList<InflictedStatus>? inflicts = null,
        string? statusAbilityName = null, DurationPolicy durationPolicy = DurationPolicy.Independent,
        string? defaultDurationAmount = null, string? defaultAmount = null,
        ValueCombinationFunction combinationFunction = ValueCombinationFunction.Subtract,
        EffectApplicationHandler? applicationHandler = null,
        bool restoresActivator = false,
        bool extendsOnMovement = false)
        => Effect.RegisterMetadata(new EffectMetadata
        {
            EffectType = effectType,
            ResourceProperty = resourceProperty,
            RestoresActivator = restoresActivator,
            ExtendsOnMovement = extendsOnMovement,
            CombinationFunction = combinationFunction,
            ResistanceProperty = resistanceProperty,
            AbsorptionProperty = absorptionProperty,
            IncludesMagicResistance = includesMagicResistance,
            Inflicts = combinationFunction == ValueCombinationFunction.Subtract
                ? AddDamageClears(inflicts)
                : inflicts,
            StatusAbilityName = statusAbilityName,
            Duration = EffectDuration.Instant,
            DefaultDurationAmount = defaultDurationAmount,
            DefaultAmount = defaultAmount,
            DurationPolicy = durationPolicy,
            ApplicationHandler = applicationHandler ??
                new EffectApplicationHandler(static (in context) =>
                    context.System.ApplyResourceEffect(
                        context.Effect.Component, context.TargetEntity, context.ActivatorEntity,
                        context.Amount, context.Result, context.Manager, context.Pretend))
        });

    private static IReadOnlyList<InflictedStatus> AddDamageClears(
        IReadOnlyList<InflictedStatus>? inflicts)
        => inflicts == null
            ? [
                InflictedStatus.Clear(EffectType.Sedate),
                InflictedStatus.Clear(EffectType.Fascinate)
            ]
            : [
                InflictedStatus.Clear(EffectType.Sedate),
                InflictedStatus.Clear(EffectType.Fascinate),
                .. inflicts
            ];

}

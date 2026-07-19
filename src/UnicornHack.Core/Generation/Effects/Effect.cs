using CSharpScriptSerialization;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public abstract class Effect : ICSScriptSerializable
{
    public const string PropagatedAmountPlaceholder = "<propagated>";

    private Func<GameEntity, GameEntity, GameEntity?, float>? _durationFunction;

    private Func<GameEntity, GameEntity, GameEntity?, float>? _amountFunction;

    private Func<GameEntity, GameEntity, GameEntity?, float>? _secondaryAmountFunction;

    private static readonly EffectMetadata?[] _metadata = new EffectMetadata?[Enum.GetValues<EffectType>().Length];
    private static readonly Dictionary<string, EffectMetadata> _metadataByStatusAbilityName =
        new(StringComparer.Ordinal);

    /// <summary>
    ///     Registers and returns <paramref name="metadata" />, keyed by its own effect type. Called from each
    ///     concrete effect's module initializer, so an effect's facts live in its own file.
    /// </summary>
    internal static EffectMetadata RegisterMetadata(EffectMetadata metadata)
    {
        if (_metadata[(int)metadata.EffectType] != null)
        {
            throw new InvalidOperationException($"EffectType.{metadata.EffectType} metadata is already registered.");
        }

        _metadata[(int)metadata.EffectType] = metadata;
        if (metadata.StatusAbilityName is { } statusAbilityName
            && !_metadataByStatusAbilityName.TryAdd(statusAbilityName, metadata))
        {
            throw new InvalidOperationException(
                $"{nameof(EffectMetadata)} is already registered for status ability '{statusAbilityName}'.");
        }

        return metadata;
    }

    /// <summary>
    ///     The single source of truth for <paramref name="effectType" />'s behavioral facts (see
    ///     <see cref="EffectMetadata" />), replacing the former per-type switch tables. Throws when a type has
    ///     no registered metadata rather than silently returning a wrong default.
    /// </summary>
    public static EffectMetadata GetMetadata(EffectType effectType)
        => _metadata[(int)effectType]
            ?? throw new InvalidOperationException($"No {nameof(EffectMetadata)} is registered for EffectType.{effectType}.");

    /// <summary>
    ///     The metadata whose status ability has <paramref name="statusAbilityName" />.
    /// </summary>
    public static EffectMetadata GetMetadata(string statusAbilityName)
        => _metadataByStatusAbilityName.GetValueOrDefault(statusAbilityName)
            ?? throw new InvalidOperationException(
                $"No {nameof(EffectMetadata)} is registered for status ability '{statusAbilityName}'.");

    public static EffectMetadata? FindMetadata(string statusAbilityName)
        => _metadataByStatusAbilityName.GetValueOrDefault(statusAbilityName);

    public Ability ContainingAbility
    {
        get;
        set;
    } = null!;

    public bool TargetActivator
    {
        get;
        set;
    }

    public EffectDuration? Duration
    {
        get => field ?? Metadata.Duration;
        set;
    }

    public string? DurationAmount
    {
        get => field ?? (Duration is EffectDuration.UntilTimeout or EffectDuration.UntilXPGained
            ? Metadata.DefaultDurationAmount
            : null);
        set;
    }

    /// <summary>
    ///     The magnitude combination function this effect's contribution banks with; left unset falls back to
    ///     <see cref="DefaultCombinationFunction" />.
    /// </summary>
    public ValueCombinationFunction? CombinationFunction
    {
        get => field ?? DefaultCombinationFunction;
        set;
    }

    /// <summary>
    ///     The magnitude expression this effect applies (damage dealt, points healed, build-up banked, ...),
    ///     or <c>null</c> for effects that carry none; left unset falls back to the metadata default. A bare
    ///     Being property variable such as <c>$evasion</c> reads the activator and participates in dynamic
    ///     magnitude tracking for self-targeted effects. A target-qualified variable such as
    ///     <c>$target.evasion</c> reads the affected being without creating a dynamic dependency.
    /// </summary>
    public string? Amount
    {
        get => field ?? DefaultAmount;
        set;
    }

    /// <summary>
    ///     The magnitude combination function this effect's contribution banks with when several of the same
    ///     <see cref="EffectComponent.EffectType" /> fold together: the running-value combiner for damage and
    ///     property changes, or the threshold variant a build-up accumulator measures itself against. Defaults
    ///     to <see cref="ValueCombinationFunction.Sum" />; the build-up incapacitations and counters override
    ///     it. <see cref="ChangeProperty{T}" /> defaults to <see cref="ValueCombinationFunction.Default" />
    ///     and resolves the target property's declared function when applied.
    /// </summary>
    private ValueCombinationFunction DefaultCombinationFunction => Metadata.CombinationFunction;

    private string? DefaultAmount => Metadata.DefaultAmount;

    public virtual EffectComponent AddToAbility(
        AbilityComponent ability, GameManager manager,
        string? amountOverride = null, string? secondaryAmountOverride = null)
    {
        Debug.Assert(!ability.IsActive, $"Cannot add an effect to active ability {ability.EntityId}.");

        using var effectEntityReference = manager.CreateEntity();
        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
        effect.EffectType = Metadata.EffectType;
        effect.ContainingAbilityId = ability.EntityId;
        effect.ShouldTargetActivator = TargetActivator;

        if (amountOverride != null)
        {
            effect.Amount = amountOverride;
        }

        effectEntityReference.Referenced.Effect = effect;
        ConfigureEffect(effect, secondaryAmountOverride);

        return effect;
    }

    /// <summary>This effect's behavioral metadata (see <see cref="EffectMetadata" />).</summary>
    public abstract EffectMetadata Metadata { get; }

    protected virtual void ConfigureEffect(EffectComponent effect, string? secondaryAmountOverride)
    {
        effect.EffectType = Metadata.EffectType;

        var duration = Duration!.Value;
        effect.Duration = duration;

        var durationAmount = DurationAmount;
        Debug.Assert(durationAmount == null
                 || duration == EffectDuration.UntilTimeout
                 || duration == EffectDuration.UntilXPGained);
        Debug.Assert(
            duration is not (EffectDuration.UntilTimeout or EffectDuration.UntilXPGained)
            || durationAmount != null,
            $"{Metadata.EffectType} with duration {duration} requires a duration amount.");

        effect.DurationAmount = durationAmount;
        effect.CombinationFunction = CombinationFunction!.Value;

        if (durationAmount != null)
        {
            _durationFunction ??=
                EffectApplicationSystem.CreateAmountFunction(
                    durationAmount, ContainingAbility.Name, Metadata.EffectType.ToString());
            effect.DurationAmountFunction = _durationFunction;
        }

        if (effect.Amount == null)
        {
            var amount = Amount;
            effect.Amount = amount;

            if (amount != null)
            {
                effect.AmountFunction = GetAmountFunction(ContainingAbility.Name);
            }
        }

    }

    protected void SetSecondaryAmount(EffectComponent effect, string? secondaryAmount)
    {
        effect.SecondaryAmount = secondaryAmount;
        if (secondaryAmount != null)
        {
            _secondaryAmountFunction ??=
                EffectApplicationSystem.CreateAmountFunction(
                    secondaryAmount, ContainingAbility.Name, Metadata.EffectType.ToString());
            effect.SecondaryAmountFunction = _secondaryAmountFunction;
        }
    }

    internal Func<GameEntity, GameEntity, GameEntity?, float> GetAmountFunction(string abilityName)
    {
        var amount = Amount
                     ?? throw new InvalidOperationException(
                         $"EffectType.{Metadata.EffectType} has no amount expression.");
        return _amountFunction ??= EffectApplicationSystem.CreateAmountFunction(
            amount, abilityName, Metadata.EffectType.ToString());
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Effect>(GetPropertyConditions<Effect>());

    protected static Dictionary<string, Func<TEffect, object?, bool>> GetPropertyConditions<TEffect>()
        where TEffect : Effect => new()
    {
        { nameof(ContainingAbility), (_, v) => false },
        { nameof(TargetActivator), (_, v) => (bool)v! },
        { nameof(Duration), (e, v) => v is EffectDuration d && d != e.Metadata.Duration },
        { nameof(DurationAmount), (e, v) => v is string s && s != e.Metadata.DefaultDurationAmount },
        { nameof(CombinationFunction), (e, v) => v is ValueCombinationFunction f && f != e.Metadata.CombinationFunction },
        { nameof(Amount), (e, v) => v is string s && s != e.Metadata.DefaultAmount },
    };

    public virtual ICSScriptSerializer GetSerializer() => Serializer;
}

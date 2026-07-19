namespace UnicornHack.Systems.Effects;

/// <summary>
///     A uniform view of an effect being applied, whether it comes from a runtime component, an authored
///     definition applied directly, or a metadata-only status application.
/// </summary>
public readonly struct EffectApplication
{
    private readonly EffectComponent? _component;
    private readonly Effect? _definition;
    private readonly Ability? _ability;
    private readonly int? _resolvedAmount;
    private readonly int? _sourceAbilityId;

    public EffectApplication(EffectComponent component, int? resolvedAmount = null)
    {
        _component = component;
        _definition = null;
        _ability = null;
        _resolvedAmount = resolvedAmount;
        _sourceAbilityId = component.ContainingAbilityId;
        Metadata = Effect.GetMetadata(component.EffectType);
    }

    internal EffectApplication(Effect definition, Ability ability)
    {
        _component = null;
        _definition = definition;
        _ability = ability;
        _resolvedAmount = null;
        _sourceAbilityId = null;
        Metadata = definition.Metadata;
    }

    private EffectApplication(EffectType effectType, int amount, int? sourceAbilityId)
    {
        _component = null;
        _definition = null;
        _ability = null;
        _resolvedAmount = amount;
        _sourceAbilityId = sourceAbilityId;
        Metadata = Effect.GetMetadata(effectType);
    }

    internal static EffectApplication CreateDirect(
        EffectType effectType, int amount, int? sourceAbilityId = null)
        => new(effectType, amount, sourceAbilityId);

    public EffectMetadata Metadata { get; }
    public EffectType EffectType => Metadata.EffectType;
    public bool HasComponent => _component != null;
    public EffectComponent Component
        => _component ?? throw new InvalidOperationException(
            $"Direct {EffectType} application has no runtime effect component.");
    public EffectDuration Duration => _component?.Duration ?? _definition?.Duration ?? EffectDuration.Instant;
    public ValueCombinationFunction CombinationFunction
        => _component != null
            ? _component.CombinationFunction
            : _definition?.CombinationFunction ?? Metadata.CombinationFunction;
    public string? AmountExpression => _component?.Amount ?? _definition?.Amount;
    public int? SourceAbilityId => _sourceAbilityId;

    public int GetAmount(
        EffectApplicationSystem system, GameEntity activatorEntity, GameEntity? targetEntity)
    {
        if (_resolvedAmount is { } resolvedAmount)
        {
            return resolvedAmount;
        }

        if (_component != null)
        {
            return system.GetAmount(_component, activatorEntity, targetEntity);
        }

        if (_definition?.Amount == null)
        {
            return 0;
        }

        var abilityName = _ability!.Name;
        return EffectApplicationSystem.InvokeAmountFunction(
            _definition.GetAmountFunction(abilityName), targetEntity ?? activatorEntity,
            activatorEntity, targetEntity, nameof(Effect.Amount), abilityName);
    }

    public int GetDurationAmount(
        EffectApplicationSystem system, GameEntity activatorEntity, GameEntity? targetEntity)
        => _component != null
            ? system.GetDurationAmount(_component, activatorEntity, targetEntity)
            : 0;

    public int GetSecondaryAmount(
        EffectApplicationSystem system, GameEntity activatorEntity, GameEntity? targetEntity)
        => _component != null
            ? system.GetSecondaryAmount(_component, activatorEntity, targetEntity)
            : 0;
}

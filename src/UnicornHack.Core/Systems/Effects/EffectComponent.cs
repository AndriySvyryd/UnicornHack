namespace UnicornHack.Systems.Effects;

// TODO: Separate into Effect and AppliedEffect
[Component(Id = (int)EntityComponent.Effect)]
public class EffectComponent : GameComponent
{
    private int? _affectedEntityId;
    private GameEntity? _affectedEntity;
    private int? _sourceEffectId;
    private GameEntity? _sourceEffect;
    private int? _sourceAbilityId;
    private GameEntity? _sourceAbility;
    private int? _containingAbilityId;
    private string? _durationAmount;
    private int? _expirationTick;
    private int? _expirationXp;
    private bool _shouldTargetActivator;
    private int? _appliedAmount;
    private string? _amount;
    private string? _secondaryAmount;
    private EffectType _effectType;
    private ValueCombinationFunction _combinationFunction;
    private string? _targetName;
    private int? _targetEntityId;
    private EffectDuration _duration;

    public EffectComponent()
    {
        ComponentId = (int)EntityComponent.Effect;
    }

    public int? AffectedEntityId
    {
        get => _affectedEntityId;
        set => SetWithNotify(value, ref _affectedEntityId);
    }

    public GameEntity? AffectedEntity
    {
        get => _affectedEntity ??= Entity.Manager!.FindEntity(_affectedEntityId);
        set
        {
            AffectedEntityId = value?.Id;
            _affectedEntity = value;
        }
    }

    public int? SourceEffectId
    {
        get => _sourceEffectId;
        set => SetWithNotify(value, ref _sourceEffectId);
    }

    public GameEntity? SourceEffect
    {
        get => _sourceEffect ??= Entity.Manager?.FindEntity(_sourceEffectId);
        set
        {
            SourceEffectId = value?.Id;
            _sourceEffect = value;
        }
    }

    public int? SourceAbilityId
    {
        get => _sourceAbilityId;
        set => SetWithNotify(value, ref _sourceAbilityId);
    }

    public GameEntity? SourceAbility
    {
        get => _sourceAbility ??= Entity.Manager.FindEntity(_sourceAbilityId);
        set
        {
            SourceAbilityId = value?.Id;
            _sourceAbility = value;
        }
    }

    public int? ContainingAbilityId
    {
        get => _containingAbilityId;
        set => SetWithNotify(value, ref _containingAbilityId);
    }

    public GameEntity? ContainingAbility
    {
        get;
        private set;
    }

    public EffectDuration Duration
    {
        get => _duration;
        set => SetWithNotify(value, ref _duration);
    }

    public Func<GameEntity, GameEntity, float>? DurationAmountFunction
    {
        get;
        set;
    }

    public string? DurationAmount
    {
        get => _durationAmount;
        set
        {
            SetWithNotify(value, ref _durationAmount);
            DurationAmountFunction = null;
        }
    }

    public int? ExpirationTick
    {
        get => _expirationTick;
        set => SetWithNotify(value, ref _expirationTick);
    }

    public int? ExpirationXp
    {
        get => _expirationXp;
        set => SetWithNotify(value, ref _expirationXp);
    }

    public bool ShouldTargetActivator
    {
        get => _shouldTargetActivator;
        set => SetWithNotify(value, ref _shouldTargetActivator);
    }

    // The amount for an applied effect or a constant amount for an effect
    public int? AppliedAmount
    {
        get => _appliedAmount;
        set => SetWithNotify(value, ref _appliedAmount);
    }

    public Func<GameEntity, GameEntity, float>? AmountFunction
    {
        get;
        set;
    }

    // The string function to calculate the amount of an effect
    public string? Amount
    {
        get => _amount;
        set
        {
            SetWithNotify(value, ref _amount);
            AmountFunction = null;
        }
    }

    public Func<GameEntity, GameEntity, float>? SecondaryAmountFunction
    {
        get;
        set;
    }

    // The string function to calculate the secondary amount of an effect
    public string? SecondaryAmount
    {
        get => _secondaryAmount;
        set
        {
            SetWithNotify(value, ref _secondaryAmount);
            SecondaryAmountFunction = null;
        }
    }

    public EffectType EffectType
    {
        get => _effectType;
        set => SetWithNotify(value, ref _effectType);
    }

    public ValueCombinationFunction CombinationFunction
    {
        get => _combinationFunction;
        set => SetWithNotify(value, ref _combinationFunction);
    }

    public string? TargetName
    {
        get => _targetName;
        set => SetWithNotify(value, ref _targetName);
    }

    public int? TargetEntityId
    {
        get => _targetEntityId;
        set => SetWithNotify(value, ref _targetEntityId);
    }

    public EffectComponent AddToAbility(GameEntity abilityEntity)
    {
        var manager = abilityEntity.Manager;
        using var entityReference = manager.CreateEntity();
        var clone = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
        clone.ShouldTargetActivator = ShouldTargetActivator;
        clone.Duration = Duration;
        clone.DurationAmount = DurationAmount;
        clone.DurationAmountFunction = DurationAmountFunction;
        clone.AppliedAmount = AppliedAmount;
        clone.Amount = Amount;
        clone.AmountFunction = AmountFunction;
        clone.SecondaryAmount = SecondaryAmount;
        clone.SecondaryAmountFunction = SecondaryAmountFunction;
        clone.EffectType = EffectType;
        clone.CombinationFunction = CombinationFunction;
        clone.TargetName = TargetName;
        clone.TargetEntityId = TargetEntityId;

        clone.ContainingAbilityId = abilityEntity.Id;

        if (EffectType == EffectType.AddAbility)
        {
            Entity.Ability?.AddToEffect(entityReference.Referenced);
        }

        entityReference.Referenced.Effect = clone;
        return clone;
    }

    protected override void Clean()
    {
        _affectedEntityId = default;
        _affectedEntity = default;
        _sourceEffectId = default;
        _sourceEffect = default;
        _sourceAbilityId = default;
        _sourceAbility = default;
        _containingAbilityId = default;
        ContainingAbility = default;
        _duration = default;
        DurationAmountFunction = default;
        _durationAmount = default;
        _expirationTick = default;
        _expirationXp = default;
        _shouldTargetActivator = default;
        _appliedAmount = default;
        AmountFunction = default;
        _amount = default;
        SecondaryAmountFunction = default;
        _secondaryAmount = default;
        _effectType = default;
        _combinationFunction = default;
        _targetName = default;
        _targetEntityId = default;

        base.Clean();
    }
}

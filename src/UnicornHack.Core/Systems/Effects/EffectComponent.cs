namespace UnicornHack.Systems.Effects;

[Component(Id = (int)EntityComponent.Effect)]
public class EffectComponent : GameComponent
{
    private int? _sourceEffectId;
    private GameEntity? _sourceEffect;
    private int? _sourceAbilityId;
    private GameEntity? _sourceAbility;
    private int? _containingAbilityId;
    private string? _durationAmount;
    private bool _shouldTargetActivator;
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

    public int? SourceEffectId
    {
        get => _sourceEffectId;
        set => SetWithNotify(value, ref _sourceEffectId);
    }

    public GameEntity? SourceEffect
    {
        get
        {
            _sourceEffect ??= Entity.Manager?.FindEntity(_sourceEffectId);
            Debug.Assert(_sourceEffect != null || _sourceEffectId == null,
                $"SourceEffect {_sourceEffectId} not loaded");
            return _sourceEffect;
        }
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
        get
        {
            _sourceAbility ??= Entity.Manager.FindEntity(_sourceAbilityId);
            Debug.Assert(_sourceAbility != null || _sourceAbilityId == null,
                $"SourceAbility {_sourceAbilityId} not loaded");
            return _sourceAbility;
        }
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

    public Func<GameEntity, GameEntity, GameEntity?, float>? DurationAmountFunction
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

    public bool ShouldTargetActivator
    {
        get => _shouldTargetActivator;
        set => SetWithNotify(value, ref _shouldTargetActivator);
    }

    public Func<GameEntity, GameEntity, GameEntity?, float>? AmountFunction
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

    public Func<GameEntity, GameEntity, GameEntity?, float>? SecondaryAmountFunction
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

    /// <summary>
    ///     The target entity or embedded-effect index. Effect-specific negative values encode selectors;
    ///     <see cref="DecreaseAbilityLevel" /> stores <c>-1 - AbilityType</c> here.
    /// </summary>
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
        clone.EffectType = EffectType;
        clone.ShouldTargetActivator = ShouldTargetActivator;
        clone.Duration = Duration;
        clone.DurationAmount = DurationAmount;
        clone.DurationAmountFunction = DurationAmountFunction;
        clone.Amount = Amount;
        clone.AmountFunction = AmountFunction;
        clone.SecondaryAmount = SecondaryAmount;
        clone.SecondaryAmountFunction = SecondaryAmountFunction;
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
        _sourceEffectId = default;
        _sourceEffect = default;
        _sourceAbilityId = default;
        _sourceAbility = default;
        _containingAbilityId = default;
        ContainingAbility = default;
        _duration = default;
        DurationAmountFunction = default;
        _durationAmount = default;
        _shouldTargetActivator = default;
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

using System;
using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Systems.Abilities;

[Component(Id = (int)EntityComponent.Ability)]
public class AbilityComponent : GameComponent
{
    private string _name;
    private AbilityType _type;
    private int _level;
    private GameEntity _ownerEntity;
    private int? _ownerId;
    private ActivationType _activation;
    private int? _activationCondition;
    private ActivationType _trigger;
    private int _minHeadingDeviation;
    private int _maxHeadingDeviation;
    private int _range;
    private int _targetingShapeSize = 1;
    private TargetingShape _targetingShape;
    private AbilityAction _action;
    private AbilitySuccessCondition _successCondition;
    private string _accuracy;
    private int _cooldown;
    private int _xpCooldown;
    private int? _cooldownTick;
    private int? _cooldownXpLeft;
    private int _energyCost;
    private string _delay;
    private bool _isActive;
    private bool _isUsable = true;
    private int? _slot;
    private Ability _template;
    private bool _templateLoaded;

    public AbilityComponent()
    {
        ComponentId = (int)EntityComponent.Ability;
    }

    public string Name
    {
        get => _name;
        set => SetWithNotify(value, ref _name);
    }

    public Ability Template
    {
        get
        {
            if (_templateLoaded
                || _name == null)
            {
                return _template;
            }

            _template = Ability.Loader.Find(_name);
            _templateLoaded = true;
            return _template;
        }

        set
        {
            _template = value;
            _templateLoaded = true;
        }
    }

    public AbilityType Type
    {
        get => _type;
        set => SetWithNotify(value, ref _type);
    }

    public int Level
    {
        get => _level;
        set => SetWithNotify(value, ref _level);
    }

    public GameEntity OwnerEntity
    {
        get => _ownerEntity ??= Entity.Manager.FindEntity(_ownerId);
        set
        {
            OwnerId = value?.Id;
            _ownerEntity = value;
        }
    }

    public int? OwnerId
    {
        get => _ownerId;
        set
        {
            _ownerEntity = null;
            SetWithNotify(value, ref _ownerId);
        }
    }

    public ActivationType Activation
    {
        get => _activation;
        set => SetWithNotify(value, ref _activation);
    }

    public AbilityAction Action
    {
        get => _action;
        set => SetWithNotify(value, ref _action);
    }

    public int? ActivationCondition
    {
        get => _activationCondition;
        set => SetWithNotify(value, ref _activationCondition);
    }

    public ActivationType Trigger
    {
        get => _trigger;
        set => SetWithNotify(value, ref _trigger);
    }

    /// <summary>
    ///     Min number of octants between heading and direction to target
    /// </summary>
    public int MinHeadingDeviation
    {
        get => _minHeadingDeviation;
        set => SetWithNotify(value, ref _minHeadingDeviation);
    }

    /// <summary>
    ///     Max number of octants between heading and direction to target
    /// </summary>
    public int MaxHeadingDeviation
    {
        get => _maxHeadingDeviation;
        set => SetWithNotify(value, ref _maxHeadingDeviation);
    }

    public int Range
    {
        get => _range;
        set => SetWithNotify(value, ref _range);
    }

    public int TargetingShapeSize
    {
        get => _targetingShapeSize;
        set => SetWithNotify(value, ref _targetingShapeSize);
    }

    public TargetingShape TargetingShape
    {
        get => _targetingShape;
        set => SetWithNotify(value, ref _targetingShape);
    }

    public AbilitySuccessCondition SuccessCondition
    {
        get => _successCondition;
        set => SetWithNotify(value, ref _successCondition);
    }

    public Func<GameEntity, GameEntity, float> AccuracyFunction
    {
        get;
        set;
    }

    public string Accuracy
    {
        get => _accuracy;
        set
        {
            SetWithNotify(value, ref _accuracy);
            AccuracyFunction = null;
        }
    }

    public int Cooldown
    {
        get => _cooldown;
        set => SetWithNotify(value, ref _cooldown);
    }

    public int XPCooldown
    {
        get => _xpCooldown;
        set => SetWithNotify(value, ref _xpCooldown);
    }

    public int? CooldownTick
    {
        get => _cooldownTick;
        set => SetWithNotify(value, ref _cooldownTick);
    }

    public int? CooldownXpLeft
    {
        get => _cooldownXpLeft;
        set => SetWithNotify(value, ref _cooldownXpLeft);
    }

    public int EnergyCost
    {
        get => _energyCost;
        set => SetWithNotify(value, ref _energyCost);
    }

    public Func<GameEntity, GameEntity, float> DelayFunction
    {
        get;
        set;
    }

    public string Delay
    {
        get => _delay;
        set
        {
            SetWithNotify(value, ref _delay);
            DelayFunction = null;
        }
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetWithNotify(value, ref _isActive);
    }

    public bool IsUsable
    {
        get => _isUsable;
        set => SetWithNotify(value, ref _isUsable);
    }

    public int? Slot
    {
        get => _slot;
        set => SetWithNotify(value, ref _slot);
    }

    public IReadOnlyCollection<GameEntity> Effects
    {
        get;
        private set;
    } = new HashSet<GameEntity>();

    public AbilityComponent AddToEffect(GameEntity abilityEffectEntity, bool includeEffects = true)
    {
        var manager = abilityEffectEntity.Manager;
        var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
        ability.Name = Name;
        ability.Type = Type;
        ability.Level = Level;
        ability.Activation = Activation;
        ability.ActivationCondition = ActivationCondition;
        ability.Trigger = Trigger;
        ability.MinHeadingDeviation = MinHeadingDeviation;
        ability.MaxHeadingDeviation = MaxHeadingDeviation;
        ability.Range = Range;
        ability.TargetingShapeSize = TargetingShapeSize;
        ability.TargetingShape = TargetingShape;
        ability.Action = Action;
        ability.SuccessCondition = SuccessCondition;
        ability.Cooldown = Cooldown;
        ability.Accuracy = Accuracy;
        ability.XPCooldown = XPCooldown;
        ability.Delay = Delay;
        ability.EnergyCost = EnergyCost;

        abilityEffectEntity.Ability = ability;

        if (includeEffects)
        {
            foreach (var effectEntity in Effects)
            {
                effectEntity.Effect.AddToAbility(abilityEffectEntity);
            }
        }

        return ability;
    }

    protected override void Clean()
    {
        _name = default;
        _type = default;
        _level = default;
        _ownerId = default;
        _ownerEntity = default;
        _activation = default;
        _activationCondition = default;
        _trigger = default;
        _minHeadingDeviation = default;
        _maxHeadingDeviation = default;
        _range = default;
        _targetingShapeSize = default;
        _targetingShape = default;
        _action = default;
        _successCondition = default;
        _accuracy = default;
        AccuracyFunction = default;
        _cooldown = default;
        _xpCooldown = default;
        _cooldownTick = default;
        _cooldownXpLeft = default;
        _energyCost = default;
        DelayFunction = default;
        _delay = default;
        _isActive = default;
        _isUsable = true;
        _slot = default;
        ((HashSet<GameEntity>)Effects).Clear();

        base.Clean();
    }
}

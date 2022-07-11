using CSharpScriptSerialization;
using UnicornHack.Data.Abilities;
using UnicornHack.Systems.Abilities;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation;

public class Ability : ILoadable, ICSScriptSerializable
{
    private Func<GameEntity, GameEntity, float>? _accuracyFunction;
    private Func<GameEntity, GameEntity, float>? _delayFunction;
    private IReadOnlyList<Effect>? _effects;

    public string Name
    {
        get;
        set;
    } = null!;

    // TODO: Move to LanguageService
    public string? EnglishDescription
    {
        get;
        set;
    }

    public AbilityType Type
    {
        get;
        set;
    }

    public int Cost
    {
        get;
        set;
    }

    public ActivationType Activation
    {
        get;
        set;
    }

    public int? ActivationCondition
    {
        get;
        set;
    }

    public ActivationType ItemCondition
    {
        get;
        set;
    }

    public ActivationType Trigger
    {
        get;
        set;
    }

    public int MinHeadingDeviation
    {
        get;
        set;
    }

    public int MaxHeadingDeviation
    {
        get;
        set;
    }

    public int Range
    {
        get;
        set;
    }

    public int TargetingShapeSize
    {
        get;
        set;
    } = 1;

    public TargetingShape TargetingShape
    {
        get;
        set;
    }

    public AbilityAction Action
    {
        get;
        set;
    }

    public AbilitySuccessCondition SuccessCondition
    {
        get;
        set;
    }

    public string? Accuracy
    {
        get;
        set;
    }

    /// <summary>
    ///     Amount of ticks that need to pass after the ability has been used or deactivated before it can be used again.
    /// </summary>
    public int Cooldown
    {
        get;
        set;
    }

    /// <summary>
    ///     Amount of experience points adjusted by level depth that a player need to gain after the
    ///     ability has been used or deactivated before it can be used again.
    /// </summary>
    public int XPCooldown
    {
        get;
        set;
    }

    public string? Delay
    {
        get;
        set;
    }

    public int EnergyPointCost
    {
        get;
        set;
    }
    
    [MaybeNull]
    public IReadOnlyList<Effect> Effects
    {
        get => _effects;
        set
        {
            _effects = value;
            foreach (var effect in value)
            {
                effect.ContainingAbility = this;
            }
        }
    }

    public virtual AbilityComponent AddToEffect(GameEntity effectEntity, int level = 0)
    {
        var manager = effectEntity.Manager;
        var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
        ability.Name = Name;
        ability.Type = Type;
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
        ability.Accuracy = Accuracy;
        ability.Cooldown = Cooldown;
        ability.XPCooldown = XPCooldown;
        ability.Delay = Delay;
        ability.EnergyCost = EnergyPointCost;
        ability.Template = this;
        ability.Level = level;

        if (Accuracy != null)
        {
            _accuracyFunction ??= AbilityActivationSystem.CreateAccuracyFunction(Accuracy, Name);

            ability.AccuracyFunction = _accuracyFunction;
        }

        if (Delay != null)
        {
            _delayFunction ??= AbilityActivationSystem.CreateDelayFunction(Delay, Name);

            ability.DelayFunction = _delayFunction;
        }

        effectEntity.Ability = ability;
        AddEffects(Effects, ability, manager);

        return ability;
    }

    protected static void AddEffects(IReadOnlyList<Effect>? effects, AbilityComponent ability, GameManager manager)
    {
        if (effects != null)
        {
            foreach (var effect in effects)
            {
                effect.AddToAbility(ability, manager);
            }
        }
    }

    public AbilityComponent AddToAffectable(GameEntity affectableEntity, int level = 0)
    {
        var manager = affectableEntity.Manager;
        using var abilityEntityReference = manager.CreateEntity();
        var ability = AddToEffect(abilityEntityReference.Referenced, level);
        ability.OwnerId = affectableEntity.Id;

        return ability;
    }

    public static readonly GroupedCSScriptLoader<AbilityType, Ability> Loader =
        new(@"Data\Abilities\", c => c.Type, typeof(AbilityData));

    private static readonly PropertyCSScriptSerializer<Ability> Serializer =
        new(GetPropertyConditions<Ability>());

    protected static Dictionary<string, Func<TAbility, object?, bool>> GetPropertyConditions<TAbility>()
        where TAbility : Ability => new()
    {
        { nameof(Name), (_, v) => v != default },
        { nameof(EnglishDescription), (_, v) => v != default },
        { nameof(Type), (_, v) => (AbilityType)v! != default },
        { nameof(Cost), (_, v) => (int)v! != default },
        { nameof(Activation), (_, v) => (ActivationType)v! != default },
        { nameof(ActivationCondition), (_, v) => v != default },
        { nameof(ItemCondition), (_, v) => (ActivationType)v! != default },
        { nameof(Trigger), (_, v) => (ActivationType)v! != default },
        { nameof(MinHeadingDeviation), (_, v) => (int)v! != default },
        { nameof(MaxHeadingDeviation), (_, v) => (int)v! != default },
        { nameof(Range), (_, v) => (int)v! != default },
        { nameof(TargetingShapeSize), (_, v) => (int)v! != 1 },
        { nameof(TargetingShape), (_, v) => (TargetingShape)v! != default },
        { nameof(Action), (_, v) => (AbilityAction)v! != default },
        { nameof(SuccessCondition), (_, v) => (AbilitySuccessCondition)v! != default },
        { nameof(Accuracy), (_, v) => v != default },
        { nameof(Cooldown), (_, v) => (int)v! != default },
        { nameof(XPCooldown), (_, v) => (int)v! != default },
        { nameof(Delay), (_, v) => (string)v! != default },
        { nameof(EnergyPointCost), (_, v) => (int)v! != default },
        { nameof(Effects), (_, v) => (((IReadOnlyList<Effect>)v!)?.Count ?? 0) != 0 }
    };

    public virtual ICSScriptSerializer GetSerializer() => Serializer;
}

using CSharpScriptSerialization;
using System.Globalization;
using UnicornHack.Systems.Abilities;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation;

public class Ability : ILoadable, ICSScriptSerializable
{
    private Func<GameEntity, GameEntity, float>? _accuracyFunction;
    private Func<GameEntity, GameEntity, float>? _delayFunction;
    private Func<GameEntity, int>? _levelFunction;
    private List<Ability>? _levelExpressionDependents;
    private IReadOnlyList<Effect>? _effects;
    private ActivationType? _activation;
    private string? _levelExpression;

    public string Name
    {
        get;
        set;
    } = null!;

    public AbilityType Type
    {
        get;
        set;
    }

    /// <summary>
    ///     Computes a level granted by the owner's other abilities. A non-positive result grants nothing.
    /// </summary>
    public string? LevelExpression
    {
        get => _levelExpression;
        set
        {
            _levelExpression = value;
            _levelFunction = null;
        }
    }

    public IReadOnlyList<Ability> LevelExpressionDependents => _levelExpressionDependents ?? [];

    internal void SetLevelFunction(Func<GameEntity, int> function) => _levelFunction = function;

    public int GetExpressionLevel(GameEntity owner)
        => _levelFunction?.Invoke(owner)
           ?? throw new InvalidOperationException(
               $"The level expression for '{Name}' has not been prepared.");

    internal void ClearLevelExpressionDependents() => _levelExpressionDependents?.Clear();

    internal void AddLevelExpressionDependent(Ability dependent)
        => (_levelExpressionDependents ??= []).Add(dependent);

    internal static bool TryGetLeveledTemplateName(
        string abilityName, out string templateName, out int level)
    {
        var separator = abilityName.LastIndexOf('#');
        if (separator > 0
            && int.TryParse(
                abilityName.AsSpan(separator + 1), NumberStyles.None,
                CultureInfo.InvariantCulture, out level))
        {
            templateName = abilityName[..separator];
            return true;
        }

        templateName = abilityName;
        level = 0;
        return false;
    }

    internal static Ability? FindTemplate(GameManager manager, string abilityName)
    {
        TryGetLeveledTemplateName(abilityName, out var templateName, out _);
        return manager.Game.Services.Data.Abilities.Find(templateName)
               ?? manager.Game.Services.Data.PlayerRaces
                   .FindByIndex(templateName)?.GetRaceAbilityTemplate()
               ?? manager.Game.Services.Data.Creatures
                   .FindByIndex(templateName)?.GetRaceAbilityTemplate();
    }

    public virtual int MaxLevel => 1;

    public virtual bool ShouldCreateCarrier(int firstLevel, int lastLevel, int effectiveLevel)
        => firstLevel <= MaxLevel;

    public int Cost
    {
        get;
        set;
    }

    public ActivationType Activation
    {
        get => _activation ?? GetDefaultActivation(Type);
        set => _activation = value;
    }

    private static ActivationType GetDefaultActivation(AbilityType type)
        => type.IsStatusLike()
           || type.IsAdaptation()
           || type is AbilityType.Skill or AbilityType.Faith or AbilityType.Race or AbilityType.Innate
                ? ActivationType.Always
                : ActivationType.Default;

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

    public int? Noise
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
    ///     Absolute experience points a player must gain after the ability is used or deactivated before
    ///     it can be used again.
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
    public virtual IReadOnlyList<Effect> Effects
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

    public AbilityComponent CreateAbility(GameEntity effectEntity, int level)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("An ability must have a name before it can be instantiated.");
        }

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
        ability.Noise = Noise;
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
        return ability;
    }

    public virtual AbilityComponent? AddToEffect(
        GameEntity effectEntity, int firstLevel = 0, int lastLevel = 0,
        string? propagatedAmount = null, string? propagatedSecondaryAmount = null)
    {
        firstLevel = firstLevel == 0 ? 1 : firstLevel;
        lastLevel = lastLevel == 0 ? MaxLevel : lastLevel;
        if (firstLevel <= 0
            || firstLevel > lastLevel)
        {
            throw new ArgumentOutOfRangeException(
                nameof(firstLevel), $"Invalid level range {firstLevel}..{lastLevel} for {Name}.");
        }

        if (firstLevel > 1)
        {
            return null;
        }

        var ability = CreateAbility(effectEntity, lastLevel);
        AddEffects(Effects, ability, effectEntity.Manager, propagatedAmount, propagatedSecondaryAmount);
        return ability;
    }

    protected static void AddEffects(
        IReadOnlyList<Effect>? effects, AbilityComponent ability, GameManager manager,
        string? propagatedAmount = null, string? propagatedSecondaryAmount = null)
    {
        if (effects != null)
        {
            foreach (var effect in effects)
            {
                effect.ContainingAbility = ability.Template!;
                effect.AddToAbility(
                    ability, manager,
                    effect.Amount == Effect.PropagatedAmountPlaceholder
                        ? propagatedAmount
                        : null,
                    propagatedSecondaryAmount);
            }
        }
    }

    public AbilityComponent AddToAffectable(GameEntity affectableEntity, int level = 1)
    {
        var manager = affectableEntity.Manager;
        using var abilityEntityReference = manager.CreateEntity();
        var ability = AddToEffect(abilityEntityReference.Referenced, 1, level)!;
        ability.OwnerId = affectableEntity.Id;

        return ability;
    }

    public static readonly AbilityLoader Loader = new(@"Data\Abilities\");

    private static readonly PropertyCSScriptSerializer<Ability> Serializer =
        new(GetPropertyConditions<Ability>());

    protected static Dictionary<string, Func<TAbility, object?, bool>> GetPropertyConditions<TAbility>()
        where TAbility : Ability => new()
    {
        { nameof(Name), (_, v) => v != default },
        { nameof(Type), (_, v) => (AbilityType)v! != default },
        { nameof(LevelExpression), (_, v) => v != default },
        { nameof(Cost), (_, v) => (int)v! != default },
        { nameof(Activation), (o, v) => (ActivationType)v! != GetDefaultActivation(o.Type) },
        { nameof(ActivationCondition), (_, v) => v != default },
        { nameof(ItemCondition), (_, v) => (ActivationType)v! != default },
        { nameof(Trigger), (_, v) => (ActivationType)v! != default },
        { nameof(MinHeadingDeviation), (_, v) => (int)v! != default },
        { nameof(MaxHeadingDeviation), (_, v) => (int)v! != default },
        { nameof(Range), (_, v) => (int)v! != default },
        { nameof(TargetingShapeSize), (_, v) => (int)v! != 1 },
        { nameof(TargetingShape), (_, v) => (TargetingShape)v! != default },
        { nameof(Action), (_, v) => (AbilityAction)v! != default },
        { nameof(Noise), (_, v) => v != null },
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

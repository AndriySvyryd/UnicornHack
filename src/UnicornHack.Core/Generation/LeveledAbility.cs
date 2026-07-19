using CSharpScriptSerialization;
using UnicornHack.Systems.Abilities;

namespace UnicornHack.Generation;

public class LeveledAbility : Ability
{
    private IReadOnlyDictionary<int, IReadOnlyList<Effect>> _leveledEffects = null!;

    public bool Cumulative
    {
        get;
        set;
    } = true;

    public IReadOnlyDictionary<int, IReadOnlyList<Effect>> LeveledEffects
    {
        get => _leveledEffects;
        set
        {
            _leveledEffects = value;
            foreach (var effects in value.Values)
            {
                foreach (var effect in effects)
                {
                    effect.ContainingAbility = this;
                }
            }
        }
    }

    public override IReadOnlyList<Effect> Effects
    {
        get => LeveledEffects.OrderBy(level => level.Key)
            .SelectMany(level => level.Value)
            .ToList();
        set => throw new InvalidOperationException(
            $"{nameof(LeveledAbility)} effects must be assigned through {nameof(LeveledEffects)}.");
    }

    public override int MaxLevel
        => LeveledEffects.Count == 0 ? 1 : LeveledEffects.Keys.Max();

    public override bool ShouldCreateCarrier(int firstLevel, int lastLevel, int effectiveLevel)
        => firstLevel <= MaxLevel
           && (Cumulative || lastLevel == Math.Min(effectiveLevel, MaxLevel));

    public override AbilityComponent? AddToEffect(
        GameEntity effectEntity, int firstLevel = 0, int lastLevel = 0,
        string? propagatedAmount = null, string? propagatedSecondaryAmount = null)
    {
        firstLevel = firstLevel == 0 ? 1 : firstLevel;
        lastLevel = lastLevel == 0 ? firstLevel : lastLevel;
        if (firstLevel <= 0
            || firstLevel > lastLevel)
        {
            throw new ArgumentOutOfRangeException(
                nameof(firstLevel), $"Invalid level range {firstLevel}..{lastLevel} for {Name}.");
        }

        if (firstLevel > MaxLevel)
        {
            return null;
        }

        var ability = CreateAbility(effectEntity, lastLevel);
        var manager = ability.Entity.Manager;
        if (Cumulative)
        {
            for (var i = firstLevel; i <= lastLevel; i++)
            {
                if (LeveledEffects.TryGetValue(i, out var effects))
                {
                    AddEffects(
                        effects, ability, manager,
                        propagatedAmount, propagatedSecondaryAmount);
                }
            }
        }
        else if (LeveledEffects.TryGetValue(lastLevel, out var effects))
        {
            AddEffects(
                effects, ability, manager,
                propagatedAmount, propagatedSecondaryAmount);
        }

        ability.Level = lastLevel;

        return ability;
    }

    private static readonly PropertyCSScriptSerializer<LeveledAbility> Serializer =
        new(GetPropertyConditions<LeveledAbility>());

    protected static new Dictionary<string, Func<TAbility, object?, bool>>
        GetPropertyConditions<TAbility>() where TAbility : Ability
    {
        var propertyConditions = Ability.GetPropertyConditions<TAbility>();

        propertyConditions[nameof(Effects)] = (_, _) => false;
        propertyConditions.Add(nameof(Cumulative), (_, v) => (bool)v! != true);
        propertyConditions.Add(nameof(LeveledEffects),
            (o, v) => v != null && ((IReadOnlyDictionary<int, IReadOnlyList<Effect>>)v).Count != 0);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}

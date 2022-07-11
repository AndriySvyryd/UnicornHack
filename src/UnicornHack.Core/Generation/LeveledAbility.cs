using CSharpScriptSerialization;
using UnicornHack.Systems.Abilities;

namespace UnicornHack.Generation;

public class LeveledAbility : Ability
{
    public bool Cumulative
    {
        get;
        set;
    }

    public IReadOnlyDictionary<int, IReadOnlyList<Effect>> LeveledEffects
    {
        get;
        set;
    } = null!;

    public override AbilityComponent AddToEffect(GameEntity effectEntity, int level = 0)
    {
        var ability = base.AddToEffect(effectEntity, level);
        if (level == 0
            && Activation != ActivationType.WhileAboveLevel)
        {
            throw new InvalidOperationException(
                $"Can't instantiate the leveled ability {Name} without specifying a level");
        }

        var manager = ability.Entity.Manager;
        if (Cumulative)
        {
            for (var i = 1; i <= level; i++)
            {
                if (LeveledEffects.TryGetValue(i, out var effects))
                {
                    AddEffects(effects, ability, manager);
                }
            }
        }
        else if (LeveledEffects.TryGetValue(level, out var effects))
        {
            AddEffects(effects, ability, manager);
        }

        ability.Level = level;

        return ability;
    }

    private static readonly PropertyCSScriptSerializer<LeveledAbility> Serializer =
        new(GetPropertyConditions<LeveledAbility>());

    protected static new Dictionary<string, Func<TAbility, object?, bool>>
        GetPropertyConditions<TAbility>() where TAbility : Ability
    {
        var propertyConditions = Ability.GetPropertyConditions<TAbility>();

        propertyConditions.Add(nameof(Cumulative), (_, v) => (bool)v! != default);
        propertyConditions.Add(nameof(LeveledEffects),
            (o, v) => v != null && ((IReadOnlyDictionary<int, IReadOnlyList<Effect>>)v).Count != 0);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}

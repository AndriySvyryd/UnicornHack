using System.Collections.Immutable;

namespace UnicornHack.Systems.Abilities;

public class SubAttackStats
{
    public AbilitySuccessCondition SuccessCondition
    {
        get;
        set;
    }

    public int HitProbability
    {
        get;
        set;
    }

    public int Accuracy
    {
        get;
        set;
    }

    public ImmutableList<GameEntity> Effects
    {
        get;
        set;
    } = null!;
}

using System.Collections.Immutable;

namespace UnicornHack.Systems.Abilities;

public class SubAttackStats
{
    public int AbilityId
    {
        get;
        set;
    }

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

    public void Reset()
    {
        AbilityId = 0;
        SuccessCondition = default;
        HitProbability = 0;
        Accuracy = 0;
        Effects = null!;
    }
}

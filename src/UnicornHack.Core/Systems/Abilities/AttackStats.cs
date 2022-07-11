using System.Collections.Immutable;

namespace UnicornHack.Systems.Abilities;

public class AttackStats
{
    public int Delay
    {
        get;
        set;
    }

    public ImmutableList<GameEntity> SelfEffects
    {
        get;
        set;
    } = null!;

    public List<SubAttackStats> SubAttacks
    {
        get;
        set;
    } = new();
}

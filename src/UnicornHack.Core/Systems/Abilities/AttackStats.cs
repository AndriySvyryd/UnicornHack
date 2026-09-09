using System.Collections.Immutable;

namespace UnicornHack.Systems.Abilities;

public class AttackStats
{
    private readonly Stack<SubAttackStats> _unusedSubAttacks = new();

    public int Delay
    {
        get;
        set;
    }

    public ImmutableList<GameEntity> SelfEffects
    {
        get;
        set;
    } = [];

    public List<SubAttackStats> SubAttacks
    {
        get;
        set;
    } = new();

    public SubAttackStats RentSubAttack()
        => _unusedSubAttacks.TryPop(out var stats) ? stats : new SubAttackStats();

    public void Reset()
    {
        Delay = 0;
        SelfEffects = [];
        foreach (var subAttack in SubAttacks)
        {
            subAttack.Reset();
            _unusedSubAttacks.Push(subAttack);
        }

        SubAttacks.Clear();
    }
}

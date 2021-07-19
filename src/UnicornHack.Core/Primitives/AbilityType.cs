using System;

namespace UnicornHack.Primitives
{
    [Flags]
    public enum AbilityType
    {
        Default = 0,
        DefaultAttack,
        Item,
        Skill,
        Trait,
        Mutation
    }
}

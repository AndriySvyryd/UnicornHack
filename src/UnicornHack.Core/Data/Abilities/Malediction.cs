using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Abilities
{
    public static partial class AbilityData
    {
        public static readonly LeveledAbility Malediction = new LeveledAbility
        {
            Name = "malediction",
            Type = AbilityType.Skill,
            Cost = 4,
            Activation = ActivationType.Always,
            Cumulative = true,
            LeveledEffects = new Dictionary<int, HashSet<Effect>> {{1, new HashSet<Effect>()}}
        };
    }
}

using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Abilities
{
    public static partial class AbilityData
    {
        public static readonly LeveledAbility WaterSourcery = new LeveledAbility
        {
            Name = "water sourcery",
            Type = AbilityType.Skill,
            Cost = 4,
            Activation = ActivationType.Always,
            Cumulative = true,
            LeveledEffects =
                new Dictionary<int, IReadOnlyList<Effect>>
                {
                    {1, new List<Effect> {new AddAbility {Name = "ice shard"}}}
                }
        };
    }
}

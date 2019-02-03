using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Abilities
{
    public static partial class AbilityData
    {
        public static readonly Ability IceShard = new Ability
        {
            Name = "ice shard",
            Activation = ActivationType.Targeted,
            Trigger = ActivationType.OnRangedAttack,
            Range = 10,
            Action = AbilityAction.Shoot,
            SuccessCondition = AbilitySuccessCondition.Attack,
            Cooldown = 1000,
            Delay = 100,
            Effects = new HashSet<Effect> {new Freeze {Damage = 40}}
        };
    }
}

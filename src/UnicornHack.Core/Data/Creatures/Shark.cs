using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Shark = new CreatureVariant
        {
            Name = "shark",
            Species = Species.Fish,
            MovementDelay = 100,
            Weight = 1000,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 17}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"swimming", "water breathing", "limblessness", "oviparity", "no inventory"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"thick hide", 3},
                    {"largeness", Size.Large},
                    {"physical deflection", 18}
                },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 0F}
        };
    }
}
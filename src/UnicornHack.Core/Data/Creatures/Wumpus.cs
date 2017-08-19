using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Wumpus = new CreatureVariant
        {
            Name = "wumpus",
            Species = Species.Quadruped,
            SpeciesClass = SpeciesClass.Aberration,
            MovementDelay = 400,
            Weight = 2500,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "clinginess",
                    "animal body",
                    "infravisibility",
                    "handlessness",
                    "singular inventory"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"largeness", Size.Large},
                    {"physical deflection", 18},
                    {"magic resistance", 10}
                },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            Noise = ActorNoiseType.Burble
        };
    }
}
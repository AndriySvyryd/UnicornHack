using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GiantBat = new CreatureVariant
        {
            Name = "giant bat",
            Species = Species.Bat,
            SpeciesClass = SpeciesClass.Bird,
            MovementDelay = 54,
            Weight = 100,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 3}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "flight",
                    "flight control",
                    "infravisibility",
                    "animal body",
                    "handlessness",
                    "singular inventory"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"stealthiness", 3},
                    {"largeness", Size.Tiny},
                    {"physical deflection", 13}
                },
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            PreviousStageName = "bat",
            NextStageName = "vampire bat",
            Behavior = MonsterBehavior.Wandering,
            Noise = ActorNoiseType.Sqeek
        };
    }
}
using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Bat = new CreatureVariant
        {
            Name = "bat",
            Species = Species.Bat,
            SpeciesClass = SpeciesClass.Bird,
            MovementDelay = 54,
            Weight = 50,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
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
                    {"physical deflection", 12}
                },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            NextStageName = "giant bat",
            GenerationFlags = GenerationFlags.SmallGroup,
            Behavior = MonsterBehavior.Wandering,
            Noise = ActorNoiseType.Sqeek
        };
    }
}
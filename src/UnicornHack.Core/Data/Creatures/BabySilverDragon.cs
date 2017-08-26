using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant BabySilverDragon = new CreatureVariant
        {
            Name = "baby silver dragon",
            Species = Species.Dragon,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 133,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 7}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "reflection",
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
                    {"thick hide", 3},
                    {"size", 8},
                    {"physical deflection", 18},
                    {"magic resistance", 10},
                    {"weight", 1500}
                },
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            NextStageName = "silver dragon",
            Noise = ActorNoiseType.Roar
        };
    }
}
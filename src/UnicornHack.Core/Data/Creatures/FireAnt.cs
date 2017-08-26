using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant FireAnt = new CreatureVariant
        {
            Name = "fire ant",
            Species = Species.Ant,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 66,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Burn {Damage = 5}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Burn {Damage = 5}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "infravisibility",
                    "animal body",
                    "handlessness",
                    "asexuality",
                    "sliming resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"stealthiness", 3},
                    {"size", 1},
                    {"physical deflection", 17},
                    {"magic resistance", 10},
                    {"weight", 30}
                },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            GenerationFlags = GenerationFlags.SmallGroup
        };
    }
}
using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GiantAnt = new CreatureVariant
        {
            Name = "giant ant",
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
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    }
                },
            SimpleProperties = new HashSet<string> {"animal body", "handlessness", "asexuality"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"stealthiness", 3},
                    {"size", 1},
                    {"physical deflection", 17},
                    {"weight", 10}
                },
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            GenerationFlags = GenerationFlags.SmallGroup
        };
    }
}
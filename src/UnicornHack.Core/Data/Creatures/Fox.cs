using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Fox = new CreatureVariant
        {
            Name = "fox",
            Species = Species.Fox,
            SpeciesClass = SpeciesClass.Canine,
            MovementDelay = 80,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 20}}
                }
            },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties =
                new Dictionary<string, object> {{"size", 2}, {"physical deflection", 13}, {"weight", 300}},
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 7F},
            Noise = ActorNoiseType.Bark
        };
    }
}
using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Jackal = new CreatureVariant
        {
            Name = "jackal",
            Species = Species.Dog,
            SpeciesClass = SpeciesClass.Canine,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties =
                new Dictionary<string, object> {{"size", 2}, {"physical deflection", 13}, {"weight", 300}},
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Bark
        };
    }
}
using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Dingo = new CreatureVariant
        {
            Name = "dingo",
            Species = Species.Dog,
            SpeciesClass = SpeciesClass.Canine,
            MovementDelay = 75,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 3}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 13}, {"weight", 400}},
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            Noise = ActorNoiseType.Bark
        };
    }
}
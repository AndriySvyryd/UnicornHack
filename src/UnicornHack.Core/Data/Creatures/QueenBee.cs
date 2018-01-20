using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant QueenBee = new CreatureVariant
        {
            Name = "queen bee",
            Species = Species.Bee,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 50,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Sting,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Envenom {Damage = 40}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Poison {Damage = 80}}
                }
            },
            SimpleProperties =
                new HashSet<string> {"flight", "flight control", "animal body", "handlessness", "femaleness"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 75},
                {"size", 1},
                {"physical deflection", 24},
                {"weight", 5}
            },
            InitialLevel = 9,
            GenerationFlags = GenerationFlags.Entourage,
            Noise = ActorNoiseType.Buzz
        };
    }
}
using System.Collections.Generic;
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
            Weight = 5,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Sting,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new VenomDamage {Damage = 4}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 8}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"flight", "flight control", "animal body", "handlessness", "femaleness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"largeness", Size.Tiny},
                    {"physical deflection", 24}
                },
            InitialLevel = 9,
            GenerationFlags = GenerationFlags.Entourage,
            Noise = ActorNoiseType.Buzz
        };
    }
}
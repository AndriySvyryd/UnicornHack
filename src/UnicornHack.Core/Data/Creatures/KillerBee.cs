using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant KillerBee = new CreatureVariant
        {
            Name = "killer bee",
            Species = Species.Bee,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 66,
            Weight = 5,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Sting,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new VenomDamage {Damage = 2}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 2}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"flight", "flight control", "animal body", "handlessness", "femaleness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"largeness", Size.Tiny},
                    {"physical deflection", 21}
                },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            GenerationFlags = GenerationFlags.LargeGroup,
            Noise = ActorNoiseType.Buzz
        };
    }
}
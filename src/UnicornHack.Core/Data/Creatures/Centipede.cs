using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Centipede = new CreatureVariant
        {
            Name = "centipede",
            Species = Species.Centipede,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 300,
            Weight = 50,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
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
                new HashSet<string> {"concealment", "clinginess", "animal body", "handlessness", "oviparity"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"largeness", Size.Tiny},
                    {"physical deflection", 16}
                },
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 6F}
        };
    }
}
using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant BrownMold = new CreatureVariant
        {
            Name = "brown mold",
            Species = Species.Fungus,
            Weight = 50,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new ColdDamage {Damage = 3}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new ColdDamage {Damage = 2}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "decay resistance",
                    "breathlessness",
                    "non animal",
                    "eyelessness",
                    "limblessness",
                    "headlessness",
                    "mindlessness",
                    "asexuality",
                    "no inventory"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"cold resistance", 3},
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"stealthiness", 3},
                    {"largeness", Size.Small},
                    {"physical deflection", 11}
                },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 2F}
        };
    }
}
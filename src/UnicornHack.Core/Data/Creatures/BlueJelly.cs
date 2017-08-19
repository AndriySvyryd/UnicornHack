using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant BlueJelly = new CreatureVariant
        {
            Name = "blue jelly",
            Species = Species.Jelly,
            Weight = 100,
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
                        Effects = new HashSet<Effect> {new ColdDamage {Damage = 3}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "breathlessness",
                    "amorphism",
                    "non animal",
                    "eyelessness",
                    "limblessness",
                    "headlessness",
                    "mindlessness",
                    "asexuality",
                    "no inventory"
                },
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 3},
                {"venom resistance", 3},
                {"cold resistance", 3},
                {"stealthiness", 3},
                {"largeness", Size.Small},
                {"physical deflection", 12},
                {"magic resistance", 10}
            },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 5F}
        };
    }
}
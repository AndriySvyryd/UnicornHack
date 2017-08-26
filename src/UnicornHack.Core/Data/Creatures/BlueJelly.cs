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
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new Freeze {Damage = 3}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Freeze {Damage = 3}}
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
                {"size", 2},
                {"physical deflection", 12},
                {"magic resistance", 10},
                {"weight", 100}
            },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 5F}
        };
    }
}
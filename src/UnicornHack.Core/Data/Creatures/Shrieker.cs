using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Shrieker = new CreatureVariant
        {
            Name = "shrieker",
            Species = Species.Fungus,
            MovementDelay = 1200,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Scream,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Deafen {Duration = 3}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "breathlessness",
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
                {"size", 2},
                {"physical deflection", 13},
                {"weight", 100}
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 2F}
        };
    }
}
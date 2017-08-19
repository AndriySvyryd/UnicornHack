using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Lichen = new CreatureVariant
        {
            Name = "lichen",
            Species = Species.Fungus,
            MovementDelay = 1200,
            Weight = 20,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Stick()}
                    },
                    new Ability {Activation = AbilityActivation.OnMeleeHit, Effects = new HashSet<Effect> {new Stick()}}
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
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"stealthiness", 3},
                    {"largeness", Size.Small},
                    {"physical deflection", 11}
                },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 5F}
        };
    }
}
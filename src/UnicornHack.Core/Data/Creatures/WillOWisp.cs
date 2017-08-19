using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant WillOWisp = new CreatureVariant
        {
            Name = "will o' wisp",
            Species = Species.FloatingSphere,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 80,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Explosion,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Confuse {Duration = 27}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "flight",
                    "flight control",
                    "infravisibility",
                    "invisibility detection",
                    "non animal",
                    "non solid body",
                    "breathlessness",
                    "limblessness",
                    "eyelessness",
                    "headlessness",
                    "mindlessness",
                    "asexuality",
                    "no inventory",
                    "sliming resistance",
                    "sickness resistance"
                },
            ValuedProperties = new Dictionary<string, object>
            {
                {"fire resistance", 3},
                {"cold resistance", 3},
                {"electricity resistance", 3},
                {"acid resistance", 3},
                {"disintegration resistance", 3},
                {"poison resistance", 3},
                {"venom resistance", 3},
                {"stealthiness", 3},
                {"largeness", Size.Small},
                {"magic resistance", 20}
            },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            CorpseName = ""
        };
    }
}
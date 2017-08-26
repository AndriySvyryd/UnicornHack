using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GreenSlime = new CreatureVariant
        {
            Name = "green slime",
            Species = Species.Ooze,
            MovementDelay = 200,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Slime()}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new Slime()}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Slime()}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "decay resistance",
                    "breathlessness",
                    "amorphism",
                    "non animal",
                    "eyelessness",
                    "limblessness",
                    "headlessness",
                    "mindlessness",
                    "asexuality",
                    "stoning resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"cold resistance", 3},
                    {"electricity resistance", 3},
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"acid resistance", 3},
                    {"stealthiness", 3},
                    {"physical deflection", 14},
                    {"weight", 400}
                },
            InitialLevel = 6,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight(), Name = "hell"},
            CorpseName = ""
        };
    }
}
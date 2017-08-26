using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant IceVortex = new CreatureVariant
        {
            Name = "ice vortex",
            Species = Species.Vortex,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 60,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Engulf {Duration = 4}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnDigestion,
                        Action = AbilityAction.Digestion,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Freeze {Damage = 3}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new Freeze {Damage = 3}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnRangedHit,
                        Effects = new HashSet<Effect> {new Freeze {Damage = 3}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "flight",
                    "flight control",
                    "infravisibility",
                    "non animal",
                    "non solid body",
                    "breathlessness",
                    "limblessness",
                    "eyelessness",
                    "headlessness",
                    "mindlessness",
                    "asexuality",
                    "stoning resistance",
                    "sliming resistance",
                    "sickness resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"cold resistance", 3},
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"size", 16},
                    {"physical deflection", 18},
                    {"magic resistance", 30},
                    {"weight", 0}
                },
            InitialLevel = 5,
            GenerationWeight = new BranchWeight {NotMatched = new DefaultWeight {Multiplier = 5F}, Name = "hell"},
            CorpseName = ""
        };
    }
}
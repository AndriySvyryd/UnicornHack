using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant FogCloud = new CreatureVariant
        {
            Name = "fog cloud",
            Species = Species.Cloud,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 1200,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Engulf {Duration = 3}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnDigestion,
                        Action = AbilityAction.Digestion,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 3}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnDigestion,
                        Action = AbilityAction.Digestion,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Soak {Damage = 1}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new Soak {Damage = 2}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "flight",
                    "flight control",
                    "non animal",
                    "non solid body",
                    "breathlessness",
                    "limblessness",
                    "eyelessness",
                    "headlessness",
                    "mindlessness",
                    "asexuality",
                    "no inventory",
                    "stoning resistance",
                    "sliming resistance",
                    "sickness resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"acid resistance", 3},
                    {"stealthiness", 3},
                    {"size", 16},
                    {"magic resistance", 30},
                    {"weight", 1}
                },
            InitialLevel = 3,
            GenerationWeight = new BranchWeight {NotMatched = new DefaultWeight {Multiplier = 4F}, Name = "hell"},
            CorpseName = ""
        };
    }
}
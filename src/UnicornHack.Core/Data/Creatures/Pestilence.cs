using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Pestilence = new CreatureVariant
        {
            Name = "Pestilence",
            Species = Species.Horseman,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 100,
            Weight = 1000,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new ScriptedEffect {Script = "Pestilence"}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new ScriptedEffect {Script = "Pestilence"}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new ScriptedEffect {Script = "Pestilence"}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "decay resistance",
                    "breathlessness",
                    "reanimation",
                    "flight",
                    "flight control",
                    "teleportation control",
                    "polymorph control",
                    "infravisibility",
                    "infravision",
                    "invisibility detection",
                    "humanoidness",
                    "maleness",
                    "stoning resistance",
                    "sliming resistance",
                    "sickness resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"acid resistance", 3},
                    {"fire resistance", 3},
                    {"cold resistance", 3},
                    {"electricity resistance", 3},
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"regeneration", 3},
                    {"physical deflection", 25},
                    {"magic resistance", 100}
                },
            InitialLevel = 30,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight {Multiplier = 0F}, Name = "hell"},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.Stalking | MonsterBehavior.Displacing,
            Noise = ActorNoiseType.Rider
        };
    }
}
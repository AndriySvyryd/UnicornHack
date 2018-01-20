using System.Collections.Generic;
using UnicornHack.Abilities;
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
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new ScriptedEffect {Script = "Pestilence"}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new ScriptedEffect {Script = "Pestilence"}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new ScriptedEffect {Script = "Pestilence"}}
                }
            },
            SimpleProperties = new HashSet<string>
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
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "acid resistance",
                    75
                },
                {
                    "fire resistance",
                    75
                },
                {
                    "cold resistance",
                    75
                },
                {
                    "electricity resistance",
                    75
                },
                {
                    "poison resistance",
                    75
                },
                {
                    "venom resistance",
                    75
                },
                {
                    "regeneration",
                    3
                },
                {
                    "physical deflection",
                    25
                },
                {
                    "magic resistance",
                    100
                },
                {
                    "weight",
                    1000
                }
            },
            InitialLevel = 30,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight {Multiplier = 0F}, Name = "hell"},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.Stalking | MonsterBehavior.Displacing,
            Noise = ActorNoiseType.Rider
        };
    }
}
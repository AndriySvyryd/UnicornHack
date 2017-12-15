using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Shade = new CreatureVariant
        {
            Name = "shade",
            Species = Species.Ghost,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 120,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Paralyze {Duration = 7}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Slow {Duration = 3}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "flight",
                "flight control",
                "phasing",
                "infravision",
                "invisibility detection",
                "non solid body",
                "humanoidness",
                "breathlessness",
                "no inventory",
                "stoning resistance",
                "sliming resistance",
                "sickness resistance"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "cold resistance",
                    3
                },
                {
                    "disintegration resistance",
                    3
                },
                {
                    "poison resistance",
                    3
                },
                {
                    "physical deflection",
                    10
                },
                {
                    "magic resistance",
                    25
                },
                {
                    "weight",
                    0
                }
            },
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            PreviousStageName = "ghost",
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking,
            Noise = ActorNoiseType.Howl
        };
    }
}
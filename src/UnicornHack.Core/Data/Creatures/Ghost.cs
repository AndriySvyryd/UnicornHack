using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Ghost = new CreatureVariant
        {
            Name = "ghost",
            Species = Species.Ghost,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 400,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "flight",
                "flight control",
                "phasing",
                "infravision",
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
                    25
                },
                {
                    "magic resistance",
                    15
                },
                {
                    "weight",
                    0
                }
            },
            InitialLevel = 10,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            NextStageName = "shade",
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking
        };
    }
}
using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Ratwere = new CreatureVariant
        {
            Name = "ratwere",
            Species = Species.Rat,
            SpeciesClass = SpeciesClass.Rodent | SpeciesClass.ShapeChanger,
            MovementDelay = 100,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 5,
                    Effects = new HashSet<Effect> {new ConferLycanthropy {VariantName = "ratwere"}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new ConferLycanthropy {VariantName = "ratwere"}}
                }
            },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "poison resistance",
                    3
                },
                {
                    "regeneration",
                    3
                },
                {
                    "size",
                    2
                },
                {
                    "physical deflection",
                    14
                },
                {
                    "magic resistance",
                    10
                },
                {
                    "weight",
                    150
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Noise = ActorNoiseType.Sqeek
        };
    }
}
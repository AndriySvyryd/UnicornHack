using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Wolfwere = new CreatureVariant
        {
            Name = "wolfwere",
            Species = Species.Wolf,
            SpeciesClass = SpeciesClass.Canine | SpeciesClass.ShapeChanger,
            MovementDelay = 100,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 5,
                    Effects = new HashSet<Effect> {new ConferLycanthropy {VariantName = "wolfwere"}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new ConferLycanthropy {VariantName = "wolfwere"}}
                }
            },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 75},
                {"regeneration", 3},
                {"physical deflection", 16},
                {"magic resistance", 20},
                {"weight", 500}
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Noise = ActorNoiseType.Bark
        };
    }
}
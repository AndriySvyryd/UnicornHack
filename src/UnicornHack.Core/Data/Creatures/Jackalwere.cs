using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Jackalwere = new CreatureVariant
        {
            Name = "jackalwere",
            Species = Species.Dog,
            SpeciesClass = SpeciesClass.Canine | SpeciesClass.ShapeChanger,
            MovementDelay = 100,
            Weight = 300,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 5,
                        Effects = new HashSet<Effect> {new ConferLycanthropy {VariantName = "jackalwere"}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new ConferLycanthropy {VariantName = "jackalwere"}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"regeneration", 3},
                    {"largeness", Size.Small},
                    {"physical deflection", 13},
                    {"magic resistance", 10}
                },
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Noise = ActorNoiseType.Bark
        };
    }
}
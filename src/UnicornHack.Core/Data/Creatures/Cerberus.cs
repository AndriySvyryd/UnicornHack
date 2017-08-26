using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Cerberus = new CreatureVariant
        {
            Name = "Cerberus",
            Species = Species.Dog,
            SpeciesClass = SpeciesClass.Canine,
            MovementDelay = 120,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "animal body",
                    "infravisibility",
                    "handlessness",
                    "maleness",
                    "singular inventory"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"size", 8},
                    {"physical deflection", 18},
                    {"magic resistance", 20},
                    {"weight", 1000}
                },
            InitialLevel = 13,
            GenerationWeight =
                new BranchWeight {Matched = new InstancesWeight {W = new DefaultWeight(), Max = 1}, Name = "hell"},
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Noise = ActorNoiseType.Bark
        };
    }
}
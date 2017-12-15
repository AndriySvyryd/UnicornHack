using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant BabyPurpleDragon = new CreatureVariant
        {
            Name = "baby purple dragon",
            Species = Species.Dragon,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 133,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "flight",
                "flight control",
                "infravision",
                "animal body",
                "handlessness",
                "singular inventory"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "venom resistance",
                    3
                },
                {
                    "poison resistance",
                    3
                },
                {
                    "thick hide",
                    3
                },
                {
                    "size",
                    8
                },
                {
                    "physical deflection",
                    18
                },
                {
                    "magic resistance",
                    10
                },
                {
                    "weight",
                    1500
                }
            },
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            NextStageName = "purple dragon",
            Noise = ActorNoiseType.Roar
        };
    }
}
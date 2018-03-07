using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant PurpleDragon = new CreatureVariant
        {
            Name = "purple dragon",
            Species = Species.Dragon,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 133,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Breath,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Envenom {Damage = 140}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 90}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "flight",
                "flight control",
                "invisibility detection",
                "infravision",
                "animal body",
                "handlessness",
                "oviparity",
                "singular inventory"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "venom resistance",
                    75
                },
                {
                    "poison resistance",
                    75
                },
                {
                    "danger awareness",
                    3
                },
                {
                    "thick hide",
                    3
                },
                {
                    "size",
                    32
                },
                {
                    "physical deflection",
                    21
                },
                {
                    "magic resistance",
                    20
                },
                {
                    "weight",
                    4500
                }
            },
            InitialLevel = 15,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            PreviousStageName = "baby purple dragon",
            Behavior = MonsterBehavior.Mountable | MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector,
            Noise = ActorNoiseType.Roar
        };
    }
}
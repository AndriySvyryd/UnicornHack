using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant SilverDragon = new CreatureVariant
        {
            Name = "silver dragon",
            Species = Species.Dragon,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 133,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Breath,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 14}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 9}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "reflection",
                    "flight",
                    "flight control",
                    "invisibility detection",
                    "infravision",
                    "animal body",
                    "handlessness",
                    "oviparity",
                    "singular inventory"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"danger awareness", 3},
                    {"thick hide", 3},
                    {"size", 32},
                    {"physical deflection", 21},
                    {"magic resistance", 20},
                    {"weight", 4500}
                },
            InitialLevel = 15,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            PreviousStageName = "baby silver dragon",
            Behavior = MonsterBehavior.Mountable | MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector,
            Noise = ActorNoiseType.Roar
        };
    }
}
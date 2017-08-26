using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Xorn = new CreatureVariant
        {
            Name = "xorn",
            Species = Species.Xorn,
            MovementDelay = 133,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 14}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "phasing",
                    "breathlessness",
                    "stoning resistance",
                    "sliming resistance",
                    "sickness resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"cold resistance", 3},
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"thick hide", 3},
                    {"physical deflection", 22},
                    {"magic resistance", 20},
                    {"weight", 1200}
                },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector,
            Noise = ActorNoiseType.Roar
        };
    }
}
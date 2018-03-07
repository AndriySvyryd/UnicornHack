using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant MasterMindFlayer = new CreatureVariant
        {
            Name = "master mind flayer",
            Species = Species.Illithid,
            MovementDelay = 100,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 40}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Suck,
                    Timeout = 1,
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<int> {PropertyName = "intelligence", Value = -2, Duration = 10}
                    }
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Suck,
                    Timeout = 1,
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<int> {PropertyName = "intelligence", Value = -2, Duration = 10}
                    }
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "flight",
                "flight control",
                "invisibility detection",
                "infravision",
                "infravisibility",
                "humanoidness"
            },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"telepathy", 3},
                    {"size", 8},
                    {"magic resistance", 90},
                    {"weight", 1200}
                },
            InitialLevel = 13,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            PreviousStageName = "mind flayer",
            Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Gurgle
        };
    }
}
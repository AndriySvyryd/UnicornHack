using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant MindFlayer = new CreatureVariant
        {
            Name = "mind flayer",
            Species = Species.Illithid,
            MovementDelay = 100,
            Weight = 1200,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Suck,
                        Timeout = 1,
                        Effects = new HashSet<Effect>
                        {
                            new ChangeProperty<int> {PropertyName = "Intelligence", Value = -2, Duration = 10}
                        }
                    }
                },
            SimpleProperties =
                new HashSet<string>
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
                    {"largeness", Size.Large},
                    {"physical deflection", 15},
                    {"magic resistance", 80}
                },
            InitialLevel = 9,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            NextStageName = "master mind flayer",
            Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Gurgle
        };
    }
}
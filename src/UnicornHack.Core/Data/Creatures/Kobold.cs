using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Kobold = new CreatureVariant
        {
            Name = "kobold",
            Species = Species.Kobold,
            MovementDelay = 200,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Poison {Damage = 5}}
                    }
                },
            SimpleProperties = new HashSet<string> {"infravision", "infravisibility", "humanoidness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"size", 2},
                    {"physical deflection", 10},
                    {"weight", 400}
                },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 7F},
            NextStageName = "large kobold",
            Behavior = MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Grunt
        };
    }
}
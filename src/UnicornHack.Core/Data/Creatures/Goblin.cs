using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Goblin = new CreatureVariant
        {
            Name = "goblin",
            Species = Species.Goblin,
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
                    }
                },
            SimpleProperties = new HashSet<string> {"infravision", "infravisibility", "humanoidness"},
            ValuedProperties =
                new Dictionary<string, object> {{"size", 2}, {"physical deflection", 10}, {"weight", 400}},
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            Behavior = MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Grunt
        };
    }
}
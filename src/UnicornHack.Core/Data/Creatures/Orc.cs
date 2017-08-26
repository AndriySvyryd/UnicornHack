using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Orc = new CreatureVariant
        {
            Name = "orc",
            Species = Species.Orc,
            MovementDelay = 133,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 3}}
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
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 10}, {"weight", 1000}},
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            NextStageName = "orc captain",
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Grunt
        };
    }
}
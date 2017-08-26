using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Guard = new CreatureVariant
        {
            Name = "guard",
            Species = Species.Human,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 22}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    }
                },
            SimpleProperties = new HashSet<string> {"infravisibility", "humanoidness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"physical deflection", 10},
                    {"magic resistance", 40},
                    {"weight", 1000}
                },
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.Peaceful | MonsterBehavior.Stalking | MonsterBehavior.Displacing |
                       MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector | MonsterBehavior.Bribeable,
            Noise = ActorNoiseType.Guard
        };
    }
}
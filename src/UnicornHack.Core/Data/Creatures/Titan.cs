using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Titan = new CreatureVariant
        {
            Name = "titan",
            Species = Species.Giant,
            MovementDelay = 66,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 9}}
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
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Spell,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new MagicalDamage {Damage = 9}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"flight", "flight control", "infravision", "infravisibility", "humanoidness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"size", 32},
                    {"physical deflection", 23},
                    {"magic resistance", 70},
                    {"weight", 3000}
                },
            InitialLevel = 16,
            GenerationFlags = GenerationFlags.NonGenocidable,
            Behavior = MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
            Noise = ActorNoiseType.Boast
        };
    }
}
using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Giant = new CreatureVariant
        {
            Name = "giant",
            Species = Species.Giant,
            MovementDelay = 200,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 110}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                }
            },
            SimpleProperties = new HashSet<string> {"infravision", "infravisibility", "humanoidness"},
            ValuedProperties =
                new Dictionary<string, object> {{"size", 16}, {"physical deflection", 14}, {"weight", 2250}},
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonGenocidable,
            Behavior = MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Boast
        };
    }
}
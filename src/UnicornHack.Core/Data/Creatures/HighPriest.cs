using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant HighPriest = new CreatureVariant
        {
            Name = "high priest",
            Species = Species.Human,
            MovementDelay = 75,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 220}}
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
                    Action = AbilityAction.Spell,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new ScriptedEffect {Script = "DivineSpell"}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Spell,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new ScriptedEffect {Script = "DivineSpell"}}
                }
            },
            SimpleProperties =
                new HashSet<string> {"sleep resistance", "invisibility detection", "infravisibility", "humanoidness"},
            ValuedProperties = new Dictionary<string, object>
            {
                {
                    "fire resistance",
                    3
                },
                {
                    "electricity resistance",
                    3
                },
                {
                    "poison resistance",
                    3
                },
                {
                    "physical deflection",
                    13
                },
                {
                    "magic resistance",
                    70
                },
                {
                    "weight",
                    1000
                }
            },
            InitialLevel = 25,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.Peaceful | MonsterBehavior.Displacing | MonsterBehavior.GoldCollector |
                       MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser | MonsterBehavior.Bribeable,
            Noise = ActorNoiseType.Priest
        };
    }
}
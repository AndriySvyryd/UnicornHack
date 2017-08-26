using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Cyclops = new CreatureVariant
        {
            Name = "Cyclops",
            Species = Species.Giant,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 18}}
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
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "single eyedness",
                    "infravision",
                    "infravisibility",
                    "humanoidness",
                    "maleness",
                    "stoning resistance"
                },
            ValuedProperties = new Dictionary<string, object> {{"size", 16}, {"weight", 2200}},
            InitialLevel = 16,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.GemCollector |
                       MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser | MonsterBehavior.Covetous,
            Noise = ActorNoiseType.Quest
        };
    }
}
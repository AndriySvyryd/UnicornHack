using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Nalzok = new CreatureVariant
        {
            Name = "Nalzok",
            Species = Species.DemonMajor,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 20}}
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
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Spell,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new ScriptedEffect {Script = "ArcaneSpell"}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Poison {Damage = 5}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "flight",
                    "flight control",
                    "infravision",
                    "invisibility detection",
                    "infravisibility",
                    "maleness",
                    "humanoidness",
                    "stoning resistance",
                    "sickness resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"poison resistance", 3},
                    {"size", 16},
                    {"physical deflection", 22},
                    {"magic resistance", 75},
                    {"weight", 2500}
                },
            InitialLevel = 16,
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector |
                       MonsterBehavior.MagicUser | MonsterBehavior.Covetous,
            Noise = ActorNoiseType.Quest
        };
    }
}
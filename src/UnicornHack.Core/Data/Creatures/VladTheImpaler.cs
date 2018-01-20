using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant VladTheImpaler = new CreatureVariant
        {
            Name = "Vlad the Impaler",
            Species = Species.Vampire,
            SpeciesClass = SpeciesClass.ShapeChanger | SpeciesClass.Undead,
            MovementDelay = 66,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
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
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new DrainLife {Amount = 5}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Infect()}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "flight",
                "flight control",
                "infravision",
                "humanoidness",
                "breathlessness",
                "maleness",
                "sickness resistance"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 75},
                {"regeneration", 3},
                {"physical deflection", 23},
                {"magic resistance", 80},
                {"weight", 1000}
            },
            InitialLevel = 14,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector |
                       MonsterBehavior.Covetous
        };
    }
}
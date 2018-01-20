using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant BarrowWight = new CreatureVariant
        {
            Name = "barrow wight",
            Species = Species.Wraith,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 100,
            Abilities = new HashSet<AbilityDefinition>
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
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new DrainLife {Amount = 2}}
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
                    Effects = new HashSet<Effect> {new Infect()}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "sleep resistance",
                "infravision",
                "humanoidness",
                "breathlessness",
                "sickness resistance"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {"cold resistance", 75},
                {"poison resistance", 75},
                {"physical deflection", 15},
                {"magic resistance", 5},
                {"weight", 1200}
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            CorpseName = "",
            Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
            Noise = ActorNoiseType.Howl
        };
    }
}
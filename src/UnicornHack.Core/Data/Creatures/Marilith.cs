using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Marilith = new CreatureVariant
        {
            Name = "marilith",
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
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
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
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Poison {Damage = 3}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "infravision",
                    "invisibility detection",
                    "infravisibility",
                    "humanoid torso",
                    "serpentlike body",
                    "femaleness",
                    "sickness resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"poison resistance", 3},
                    {"size", 8},
                    {"physical deflection", 26},
                    {"magic resistance", 80},
                    {"weight", 1200}
                },
            InitialLevel = 9,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight(), Name = "hell"},
            PreviousStageName = "hezrou",
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonGenocidable,
            Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Cuss
        };
    }
}
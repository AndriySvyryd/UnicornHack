using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Salamander = new CreatureVariant
        {
            Name = "salamander",
            Species = Species.Salamander,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 100,
            Weight = 1500,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 9}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new FireDamage {Damage = 3}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Hug,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new FireDamage {Damage = 7}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Hug,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new FireDamage {Damage = 10}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new FireDamage {Damage = 10}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 7}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "infravision",
                    "infravisibility",
                    "humanoidness",
                    "serpentlike body",
                    "limblessness",
                    "sliming resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"poison resistance", 3},
                    {"thick hide", 3},
                    {"largeness", Size.Large},
                    {"physical deflection", 21}
                },
            InitialLevel = 10,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight {Multiplier = 2F}, Name = "hell"},
            Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
            Noise = ActorNoiseType.Mumble
        };
    }
}
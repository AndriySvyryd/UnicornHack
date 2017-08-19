using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GelatinousCube = new CreatureVariant
        {
            Name = "gelatinous cube",
            Species = Species.Blob,
            MovementDelay = 200,
            Weight = 600,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Paralyze {Duration = 4}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 7,
                        Effects = new HashSet<Effect> {new Engulf {Duration = 7}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnDigestion,
                        Action = AbilityAction.Digestion,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 1}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnDigestion,
                        Action = AbilityAction.Digestion,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new AcidDamage {Damage = 1}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new Paralyze {Duration = 4}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "decay resistance",
                    "breathlessness",
                    "non animal",
                    "eyelessness",
                    "limblessness",
                    "headlessness",
                    "mindlessness",
                    "asexuality",
                    "stoning resistance"
                },
            ValuedProperties = new Dictionary<string, object>
            {
                {"fire resistance", 3},
                {"cold resistance", 3},
                {"electricity resistance", 3},
                {"poison resistance", 3},
                {"venom resistance", 3},
                {"acid resistance", 3},
                {"stealthiness", 3},
                {"largeness", Size.Large},
                {"physical deflection", 12}
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            Behavior = MonsterBehavior.Wandering | MonsterBehavior.WeaponCollector
        };
    }
}
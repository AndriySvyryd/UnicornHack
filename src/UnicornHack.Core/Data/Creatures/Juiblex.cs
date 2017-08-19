using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Juiblex = new CreatureVariant
        {
            Name = "Juiblex",
            Species = Species.DemonMajor,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 400,
            Weight = 1500,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Engulf {Duration = 20}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnDigestion,
                        Action = AbilityAction.Digestion,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new AcidDamage {Damage = 22}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Spit,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new AcidDamage {Damage = 10}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 10}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new AcidDamage {Damage = 10}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "amphibiousness",
                    "flight",
                    "flight control",
                    "amorphism",
                    "headlessness",
                    "infravision",
                    "invisibility detection",
                    "maleness",
                    "stoning resistance",
                    "sickness resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"poison resistance", 3},
                    {"acid resistance", 3},
                    {"largeness", Size.Large},
                    {"physical deflection", 27},
                    {"magic resistance", 65}
                },
            InitialLevel = 30,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight {Multiplier = 0F}, Name = "hell"},
            CorpseName = "",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.Covetous,
            Noise = ActorNoiseType.Gurgle
        };
    }
}
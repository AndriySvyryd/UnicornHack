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
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Engulf {Duration = 20}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnDigestion,
                        Action = AbilityAction.Digestion,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Corrode {Damage = 22}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Spit,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Corrode {Damage = 10}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Poison {Damage = 10}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Corrode {Damage = 10}}
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
                    {"size", 8},
                    {"physical deflection", 27},
                    {"magic resistance", 65},
                    {"weight", 1500}
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
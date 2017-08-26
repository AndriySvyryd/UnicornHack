using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Wraith = new CreatureVariant
        {
            Name = "wraith",
            Species = Species.Wraith,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new DrainLife {Amount = 2}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new DrainLife {Amount = 1}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new DrainLife {Amount = 1}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "flight",
                    "flight control",
                    "infravision",
                    "non solid body",
                    "humanoidness",
                    "breathlessness",
                    "no inventory",
                    "stoning resistance",
                    "sliming resistance",
                    "sickness resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"cold resistance", 3},
                    {"poison resistance", 3},
                    {"physical deflection", 16},
                    {"magic resistance", 15},
                    {"weight", 0}
                },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            CorpseName = "",
            Behavior = MonsterBehavior.Stalking,
            Noise = ActorNoiseType.Howl
        };
    }
}
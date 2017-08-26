using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GiantMummy = new CreatureVariant
        {
            Name = "giant mummy",
            Species = Species.Giant,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 200,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 13}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Infect()}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "infravision",
                    "humanoidness",
                    "breathlessness",
                    "sickness resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"cold resistance", 3},
                    {"poison resistance", 3},
                    {"size", 16},
                    {"physical deflection", 17},
                    {"magic resistance", 20},
                    {"weight", 2250}
                },
            InitialLevel = 10,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            CorpseName = "giant",
            Noise = ActorNoiseType.Moan
        };
    }
}
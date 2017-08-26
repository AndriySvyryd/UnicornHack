using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Ghoul = new CreatureVariant
        {
            Name = "ghoul",
            Species = Species.Ghoul,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 150,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 7}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Slow {Duration = 3}}
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
                    {"physical deflection", 16},
                    {"weight", 400}
                },
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            NextStageName = "ghast",
            CorpseName = "",
            Noise = ActorNoiseType.Growl
        };
    }
}
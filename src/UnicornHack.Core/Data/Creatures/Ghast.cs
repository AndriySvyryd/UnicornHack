using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Ghast = new CreatureVariant
        {
            Name = "ghast",
            Species = Species.Ghoul,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 150,
            Weight = 400,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Paralyze {Duration = 3}}
                    },
                    new Ability
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
                    {"physical deflection", 18}
                },
            InitialLevel = 15,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            PreviousStageName = "ghoul",
            CorpseName = "",
            Noise = ActorNoiseType.Growl
        };
    }
}
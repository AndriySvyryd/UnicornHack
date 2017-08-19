using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant DwarfMummy = new CreatureVariant
        {
            Name = "dwarf mummy",
            Species = Species.Dwarf,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 200,
            Weight = 900,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 4}}
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
                    {"physical deflection", 15},
                    {"magic resistance", 30}
                },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            CorpseName = "dwarf",
            Noise = ActorNoiseType.Moan
        };
    }
}
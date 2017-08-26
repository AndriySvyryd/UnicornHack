using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant PurpleWorm = new CreatureVariant
        {
            Name = "purple worm",
            Species = Species.Worm,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 133,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 9}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Engulf {Duration = 7}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnDigestion,
                        Action = AbilityAction.Digestion,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Corrode {Damage = 5}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"serpentlike body", "eyelessness", "limblessness", "oviparity", "no inventory"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"size", 32},
                    {"physical deflection", 15},
                    {"magic resistance", 20},
                    {"weight", 1500}
                },
            InitialLevel = 15,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            PreviousStageName = "baby purple worm"
        };
    }
}
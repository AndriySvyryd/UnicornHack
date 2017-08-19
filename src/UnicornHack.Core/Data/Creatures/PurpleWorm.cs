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
            Weight = 1500,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 9}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Engulf {Duration = 7}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnDigestion,
                        Action = AbilityAction.Digestion,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new AcidDamage {Damage = 5}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"serpentlike body", "eyelessness", "limblessness", "oviparity", "no inventory"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"largeness", Size.Gigantic},
                    {"physical deflection", 15},
                    {"magic resistance", 20}
                },
            InitialLevel = 15,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            PreviousStageName = "baby purple worm"
        };
    }
}
using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant AcidBlob = new CreatureVariant
        {
            Name = "acid blob",
            Species = Species.Blob,
            MovementDelay = 400,
            Weight = 30,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new AcidDamage {Damage = 4}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new AcidDamage {Damage = 4}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "decay resistance",
                    "breathlessness",
                    "amorphism",
                    "non animal",
                    "eyelessness",
                    "limblessness",
                    "headlessness",
                    "mindlessness",
                    "asexuality",
                    "stoning resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"acid resistance", 3},
                    {"stealthiness", 3},
                    {"largeness", Size.Tiny},
                    {"physical deflection", 12}
                },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            Behavior = MonsterBehavior.Wandering
        };
    }
}
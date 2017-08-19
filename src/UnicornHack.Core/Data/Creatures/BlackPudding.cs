using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant BlackPudding = new CreatureVariant
        {
            Name = "black pudding",
            Species = Species.Pudding,
            MovementDelay = 200,
            Weight = 512,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new AcidDamage {Damage = 13}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new AcidDamage {Damage = 3}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new AcidDamage {Damage = 13}}
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
                    "reanimation",
                    "stoning resistance"
                },
            ValuedProperties = new Dictionary<string, object>
            {
                {"cold resistance", 3},
                {"electricity resistance", 3},
                {"poison resistance", 3},
                {"venom resistance", 3},
                {"acid resistance", 3},
                {"stealthiness", 3},
                {"physical deflection", 14}
            },
            InitialLevel = 10
        };
    }
}
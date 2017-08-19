using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant OchreJelly = new CreatureVariant
        {
            Name = "ochre jelly",
            Species = Species.Jelly,
            MovementDelay = 400,
            Weight = 100,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Engulf {Duration = 4}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnDigestion,
                        Action = AbilityAction.Digestion,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new AcidDamage {Damage = 10}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new AcidDamage {Damage = 7}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new AcidDamage {Damage = 7}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "breathlessness",
                    "amorphism",
                    "non animal",
                    "eyelessness",
                    "limblessness",
                    "headlessness",
                    "mindlessness",
                    "asexuality",
                    "no inventory",
                    "stoning resistance"
                },
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 3},
                {"venom resistance", 3},
                {"acid resistance", 3},
                {"stealthiness", 3},
                {"largeness", Size.Small},
                {"physical deflection", 12},
                {"magic resistance", 20}
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 5F}
        };
    }
}
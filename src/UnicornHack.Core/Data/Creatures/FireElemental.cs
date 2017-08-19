using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant FireElemental = new CreatureVariant
        {
            Name = "fire elemental",
            Species = Species.Elemental,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 100,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new FireDamage {Damage = 10}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new FireDamage {Damage = 3}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "flight",
                    "flight control",
                    "infravisibility",
                    "non animal",
                    "non solid body",
                    "breathlessness",
                    "limblessness",
                    "eyelessness",
                    "headlessness",
                    "mindlessness",
                    "asexuality",
                    "stoning resistance",
                    "sliming resistance",
                    "sickness resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"water weakness", 3},
                    {"fire resistance", 3},
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"largeness", Size.Huge},
                    {"physical deflection", 18},
                    {"magic resistance", 30}
                },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            CorpseName = ""
        };
    }
}
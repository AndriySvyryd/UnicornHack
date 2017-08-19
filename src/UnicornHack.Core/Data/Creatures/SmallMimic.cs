using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant SmallMimic = new CreatureVariant
        {
            Name = "small mimic",
            Species = Species.Mimic,
            SpeciesClass = SpeciesClass.ShapeChanger,
            MovementDelay = 400,
            Weight = 300,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 7}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Stick()}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Polymorph()}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "infravisibility",
                    "camouflage",
                    "eyelessness",
                    "headlessness",
                    "breathlessness",
                    "limblessness",
                    "clinginess",
                    "amorphism",
                    "polymorph control"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"acid resistance", 3},
                    {"stealthiness", 3},
                    {"thick hide", 3},
                    {"largeness", Size.Small},
                    {"physical deflection", 13}
                },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            NextStageName = "large mimic"
        };
    }
}
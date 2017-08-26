using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GiantMimic = new CreatureVariant
        {
            Name = "giant mimic",
            Species = Species.Mimic,
            SpeciesClass = SpeciesClass.ShapeChanger,
            MovementDelay = 400,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Stick()}
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
                    {"size", 8},
                    {"physical deflection", 13},
                    {"magic resistance", 20},
                    {"weight", 800}
                },
            InitialLevel = 9,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            PreviousStageName = "large mimic"
        };
    }
}
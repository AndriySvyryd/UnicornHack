using System.Collections.Generic;
using UnicornHack.Abilities;
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
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 70}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Stick()}
                }
            },
            SimpleProperties = new HashSet<string>
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
            ValuedProperties = new Dictionary<string, object>
            {
                {"acid resistance", 75},
                {"stealthiness", 3},
                {"thick hide", 3},
                {"size", 2},
                {"physical deflection", 13},
                {"weight", 300}
            },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            NextStageName = "large mimic"
        };
    }
}
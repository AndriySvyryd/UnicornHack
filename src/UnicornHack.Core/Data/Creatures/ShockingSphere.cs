using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant ShockingSphere = new CreatureVariant
        {
            Name = "shocking sphere",
            Species = Species.FloatingSphere,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 92,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Explosion,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Shock {Damage = 140}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "flight",
                "flight control",
                "non animal",
                "breathlessness",
                "limblessness",
                "headlessness",
                "mindlessness",
                "asexuality",
                "no inventory"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {"electricity resistance", 75},
                {"size", 2},
                {"physical deflection", 16},
                {"magic resistance", 10},
                {"weight", 10}
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            CorpseName = ""
        };
    }
}
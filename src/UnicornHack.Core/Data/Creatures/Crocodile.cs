using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Crocodile = new CreatureVariant
        {
            Name = "crocodile",
            Species = Species.Crocodile,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 133,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 100}}
                }
            },
            SimpleProperties =
                new HashSet<string> {"swimming", "amphibiousness", "handlessness", "oviparity", "singular inventory"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"thick hide", 3},
                {"size", 8},
                {"physical deflection", 16},
                {"magic resistance", 10},
                {"weight", 1500}
            },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            PreviousStageName = "baby crocodile"
        };
    }
}
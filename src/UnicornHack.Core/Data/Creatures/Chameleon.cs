using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Chameleon = new CreatureVariant
        {
            Name = "chameleon",
            Species = Species.Lizard,
            SpeciesClass = SpeciesClass.Reptile | SpeciesClass.ShapeChanger,
            MovementDelay = 200,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 60}}
                }
            },
            SimpleProperties =
                new HashSet<string> {"handlessness", "polymorph control", "oviparity", "singular inventory"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"size", 2},
                {"physical deflection", 14},
                {"magic resistance", 10},
                {"weight", 50}
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            GenerationFlags = GenerationFlags.NonPolymorphable
        };
    }
}
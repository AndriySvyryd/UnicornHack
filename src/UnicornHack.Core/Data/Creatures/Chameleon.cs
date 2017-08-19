using System.Collections.Generic;
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
            Weight = 50,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 6}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"handlessness", "polymorph control", "oviparity", "singular inventory"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"largeness", Size.Small},
                    {"physical deflection", 14},
                    {"magic resistance", 10}
                },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            GenerationFlags = GenerationFlags.NonPolymorphable
        };
    }
}
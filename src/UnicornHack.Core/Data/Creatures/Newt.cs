using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Newt = new CreatureVariant
        {
            Name = "newt",
            Species = Species.Lizard,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 200,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"swimming", "amphibiousness", "handlessness", "oviparity", "singular inventory"},
            ValuedProperties =
                new Dictionary<string, object> {{"size", 1}, {"physical deflection", 12}, {"weight", 10}},
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 5F}
        };
    }
}
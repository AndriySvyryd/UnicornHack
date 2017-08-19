using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Iguana = new CreatureVariant
        {
            Name = "iguana",
            Species = Species.Lizard,
            SpeciesClass = SpeciesClass.Reptile,
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
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    }
                },
            SimpleProperties = new HashSet<string> {"handlessness", "oviparity", "singular inventory"},
            ValuedProperties = new Dictionary<string, object> {{"largeness", Size.Small}, {"physical deflection", 13}},
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F}
        };
    }
}
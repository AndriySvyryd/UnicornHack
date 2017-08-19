using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Lizard = new CreatureVariant
        {
            Name = "lizard",
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
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 3}}
                    }
                },
            SimpleProperties = new HashSet<string> {"handlessness", "oviparity", "singular inventory"},
            ValuedProperties = new Dictionary<string, object> {{"largeness", Size.Small}, {"physical deflection", 14}},
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 4F}
        };
    }
}
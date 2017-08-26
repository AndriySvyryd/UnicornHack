using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant BabyCrocodile = new CreatureVariant
        {
            Name = "baby crocodile",
            Species = Species.Crocodile,
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
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 6}}
                    }
                },
            SimpleProperties = new HashSet<string> {"swimming", "amphibiousness", "handlessness", "singular inventory"},
            ValuedProperties =
                new Dictionary<string, object> {{"size", 2}, {"physical deflection", 14}, {"weight", 200}},
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            NextStageName = "crocodile"
        };
    }
}
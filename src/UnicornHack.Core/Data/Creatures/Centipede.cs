using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Centipede = new CreatureVariant
        {
            Name = "centipede",
            Species = Species.Centipede,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 300,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Envenom {Damage = 20}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Poison {Damage = 20}}
                }
            },
            SimpleProperties =
                new HashSet<string> {"concealment", "clinginess", "animal body", "handlessness", "oviparity"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 3},
                {"venom resistance", 3},
                {"size", 1},
                {"physical deflection", 16},
                {"weight", 50}
            },
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 6F}
        };
    }
}
using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant GiantBeetle = new CreatureVariant
        {
            Name = "giant beetle",
            Species = Species.Beetle,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 200,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Poison {Damage = 5}}
                    }
                },
            SimpleProperties = new HashSet<string> {"animal body", "handlessness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"size", 1},
                    {"physical deflection", 17},
                    {"weight", 10}
                },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 4F}
        };
    }
}
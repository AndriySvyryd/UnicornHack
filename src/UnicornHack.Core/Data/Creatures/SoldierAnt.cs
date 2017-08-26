using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant SoldierAnt = new CreatureVariant
        {
            Name = "soldier ant",
            Species = Species.Ant,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 66,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Sting,
                        Timeout = 5,
                        Effects = new HashSet<Effect> {new Envenom {Damage = 7}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new Poison {Damage = 5}}
                    }
                },
            SimpleProperties = new HashSet<string> {"animal body", "handlessness", "asexuality"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"stealthiness", 3},
                    {"size", 1},
                    {"physical deflection", 17},
                    {"weight", 20}
                },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            GenerationFlags = GenerationFlags.SmallGroup
        };
    }
}
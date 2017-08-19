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
            Weight = 20,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Sting,
                        Timeout = 5,
                        Effects = new HashSet<Effect> {new VenomDamage {Damage = 7}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 5}}
                    }
                },
            SimpleProperties = new HashSet<string> {"animal body", "handlessness", "asexuality"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"stealthiness", 3},
                    {"largeness", Size.Tiny},
                    {"physical deflection", 17}
                },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            GenerationFlags = GenerationFlags.SmallGroup
        };
    }
}
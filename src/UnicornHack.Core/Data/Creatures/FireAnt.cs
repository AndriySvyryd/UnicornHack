using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant FireAnt = new CreatureVariant
        {
            Name = "fire ant",
            Species = Species.Ant,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 66,
            Weight = 30,
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
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new FireDamage {Damage = 5}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new FireDamage {Damage = 5}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "infravisibility",
                    "animal body",
                    "handlessness",
                    "asexuality",
                    "sliming resistance"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"fire resistance", 3},
                    {"stealthiness", 3},
                    {"largeness", Size.Tiny},
                    {"physical deflection", 17},
                    {"magic resistance", 10}
                },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            GenerationFlags = GenerationFlags.SmallGroup
        };
    }
}
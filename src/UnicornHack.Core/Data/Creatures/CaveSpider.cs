using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant CaveSpider = new CreatureVariant
        {
            Name = "cave spider",
            Species = Species.Spider,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 100,
            Weight = 50,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"concealment", "clinginess", "animal body", "handlessness", "oviparity"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"venom resistance", 3},
                    {"largeness", Size.Small},
                    {"physical deflection", 17}
                },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            GenerationFlags = GenerationFlags.SmallGroup
        };
    }
}
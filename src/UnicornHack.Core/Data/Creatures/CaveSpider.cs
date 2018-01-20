using System.Collections.Generic;
using UnicornHack.Abilities;
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
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                }
            },
            SimpleProperties =
                new HashSet<string> {"concealment", "clinginess", "animal body", "handlessness", "oviparity"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 75},
                {"venom resistance", 75},
                {"size", 2},
                {"physical deflection", 17},
                {"weight", 50}
            },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            GenerationFlags = GenerationFlags.SmallGroup
        };
    }
}
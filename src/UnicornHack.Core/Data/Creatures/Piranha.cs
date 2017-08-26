using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Piranha = new CreatureVariant
        {
            Name = "piranha",
            Species = Species.Fish,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 7}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"swimming", "water breathing", "limblessness", "oviparity", "no inventory"},
            ValuedProperties =
                new Dictionary<string, object> {{"size", 1}, {"physical deflection", 16}, {"weight", 60}},
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.SmallGroup
        };
    }
}
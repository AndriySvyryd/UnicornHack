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
            Weight = 60,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 7}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"swimming", "water breathing", "limblessness", "oviparity", "no inventory"},
            ValuedProperties = new Dictionary<string, object> {{"largeness", Size.Tiny}, {"physical deflection", 16}},
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.SmallGroup
        };
    }
}
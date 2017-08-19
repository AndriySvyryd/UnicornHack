using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Wolf = new CreatureVariant
        {
            Name = "wolf",
            Species = Species.Wolf,
            SpeciesClass = SpeciesClass.Canine,
            MovementDelay = 100,
            Weight = 500,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 16}},
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Bark
        };
    }
}
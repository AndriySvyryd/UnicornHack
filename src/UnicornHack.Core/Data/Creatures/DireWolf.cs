using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant DireWolf = new CreatureVariant
        {
            Name = "dire wolf",
            Species = Species.Wolf,
            SpeciesClass = SpeciesClass.Canine,
            MovementDelay = 100,
            Weight = 1200,
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
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 17}},
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Bark
        };
    }
}
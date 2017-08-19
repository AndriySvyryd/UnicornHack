using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Pyrolisk = new CreatureVariant
        {
            Name = "pyrolisk",
            Species = Species.Cockatrice,
            SpeciesClass = SpeciesClass.MagicalBeast,
            MovementDelay = 200,
            Weight = 30,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Gaze,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new FireDamage {Damage = 7}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "animal body",
                    "infravisibility",
                    "handlessness",
                    "oviparity",
                    "singular inventory"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"fire resistance", 3},
                    {"largeness", Size.Small},
                    {"physical deflection", 14},
                    {"magic resistance", 30}
                },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Noise = ActorNoiseType.Hiss
        };
    }
}
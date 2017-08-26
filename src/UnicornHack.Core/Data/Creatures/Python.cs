using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Python = new CreatureVariant
        {
            Name = "python",
            Species = Species.Snake,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 400,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Hug,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Hug,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Bind {Duration = 2}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "swimming",
                    "infravision",
                    "serpentlike body",
                    "limblessness",
                    "oviparity",
                    "no inventory"
                },
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 15}, {"weight", 250}},
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            Noise = ActorNoiseType.Hiss
        };
    }
}
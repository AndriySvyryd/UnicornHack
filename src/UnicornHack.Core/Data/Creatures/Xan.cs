using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Xan = new CreatureVariant
        {
            Name = "xan",
            Species = Species.Xan,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 66,
            Weight = 1,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Sting,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Sting,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Cripple()}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new PoisonDamage {Damage = 5}}
                    }
                },
            SimpleProperties = new HashSet<string> {"flight", "flight control", "animal body", "handlessness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"largeness", Size.Tiny},
                    {"physical deflection", 22},
                    {"magic resistance", 20}
                },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Noise = ActorNoiseType.Buzz
        };
    }
}
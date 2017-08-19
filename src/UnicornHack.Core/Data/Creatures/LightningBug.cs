using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant LightningBug = new CreatureVariant
        {
            Name = "lightning bug",
            Species = Species.Beetle,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 100,
            Weight = 10,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new ElectricityDamage {Damage = 1}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new HashSet<Effect> {new ElectricityDamage {Damage = 1}}
                    }
                },
            SimpleProperties = new HashSet<string> {"flight", "flight control", "animal body", "handlessness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"electricity resistance", 3},
                    {"largeness", Size.Tiny},
                    {"physical deflection", 11}
                },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Buzz
        };
    }
}
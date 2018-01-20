using System.Collections.Generic;
using UnicornHack.Abilities;
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
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Shock {Damage = 10}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Shock {Damage = 10}}
                }
            },
            SimpleProperties = new HashSet<string> {"flight", "flight control", "animal body", "handlessness"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"electricity resistance", 75},
                {"size", 1},
                {"physical deflection", 11},
                {"weight", 10}
            },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Buzz
        };
    }
}
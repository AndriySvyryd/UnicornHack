using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant ElectricEel = new CreatureVariant
        {
            Name = "electric eel",
            Species = Species.Eel,
            MovementDelay = 120,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Shock {Damage = 140}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Hug,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Bind {Duration = 10}}
                }
            },
            SimpleProperties =
                new HashSet<string> {"swimming", "water breathing", "limblessness", "oviparity", "no inventory"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"electricity resistance", 75},
                {"size", 8},
                {"physical deflection", 23},
                {"weight", 600}
            },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            PreviousStageName = "giant eel"
        };
    }
}
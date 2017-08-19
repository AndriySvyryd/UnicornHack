using System.Collections.Generic;
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
            Weight = 600,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new ElectricityDamage {Damage = 14}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Hug,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Bind {Duration = 10}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"swimming", "water breathing", "limblessness", "oviparity", "no inventory"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"electricity resistance", 3},
                    {"largeness", Size.Large},
                    {"physical deflection", 23}
                },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            PreviousStageName = "giant eel"
        };
    }
}
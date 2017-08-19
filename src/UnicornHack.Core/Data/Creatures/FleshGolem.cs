using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant FleshGolem = new CreatureVariant
        {
            Name = "flesh golem",
            Species = Species.Golem,
            MovementDelay = 150,
            Weight = 1400,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 9}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 9}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "breathlessness",
                    "mindlessness",
                    "humanoidness",
                    "asexuality"
                },
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 3},
                {"regeneration", 3},
                {"health point maximum", 40},
                {"largeness", Size.Large},
                {"physical deflection", 11}
            },
            InitialLevel = 9
        };
    }
}
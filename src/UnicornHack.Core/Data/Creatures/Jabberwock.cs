using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Jabberwock = new CreatureVariant
        {
            Name = "jabberwock",
            Species = Species.Jabberwock,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 16}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 16}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 11}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "flight",
                    "flight control",
                    "infravision",
                    "infravisibility",
                    "animal body",
                    "singular inventory"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"size", 8},
                    {"physical deflection", 22},
                    {"magic resistance", 50},
                    {"weight", 1300}
                },
            InitialLevel = 15,
            Noise = ActorNoiseType.Burble
        };
    }
}
using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant RustMonster = new CreatureVariant
        {
            Name = "rust monster",
            Species = Species.RustMonster,
            MovementDelay = 66,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Soak {Damage = 70}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Soak {Damage = 70}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeHit,
                    Effects = new HashSet<Effect> {new Soak {Damage = 100}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "swimming",
                "infravisibility",
                "animal body",
                "handlessness",
                "singular inventory"
            },
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 18}, {"weight", 1000}},
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 3F}
        };
    }
}
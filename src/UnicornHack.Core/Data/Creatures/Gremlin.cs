using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Gremlin = new CreatureVariant
        {
            Name = "gremlin",
            Species = Species.Gremlin,
            MovementDelay = 100,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Curse()}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Poison {Damage = 30}}
                }
            },
            SimpleProperties = new HashSet<string> {"humanoidness", "swimming", "infravisibility"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 75},
                {"size", 2},
                {"physical deflection", 18},
                {"magic resistance", 25},
                {"weight", 100}
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            Behavior = MonsterBehavior.Stalking,
            Noise = ActorNoiseType.Laugh
        };
    }
}
using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Homunculus = new CreatureVariant
        {
            Name = "homunculus",
            Species = Species.Homunculus,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Sedate {Duration = 2}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "sleep resistance",
                    "infravision",
                    "infravisibility",
                    "mindlessness",
                    "asexuality"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"poison resistance", 3},
                    {"regeneration", 3},
                    {"size", 2},
                    {"physical deflection", 14},
                    {"magic resistance", 10},
                    {"weight", 60}
                },
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Behavior = MonsterBehavior.Stalking
        };
    }
}
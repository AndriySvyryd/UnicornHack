using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Trapper = new CreatureVariant
        {
            Name = "trapper",
            Species = Species.Trapper,
            MovementDelay = 400,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Engulf {Duration = 5}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnDigestion,
                    Action = AbilityAction.Digestion,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Suffocate()}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "camouflage",
                "animal body",
                "invisibility detection",
                "eyelessness",
                "headlessness",
                "limblessness"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {"stealthiness", 3},
                {"size", 8},
                {"physical deflection", 17},
                {"weight", 800}
            },
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Behavior = MonsterBehavior.Stalking
        };
    }
}
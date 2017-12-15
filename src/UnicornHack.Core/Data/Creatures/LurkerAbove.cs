using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant LurkerAbove = new CreatureVariant
        {
            Name = "lurker above",
            Species = Species.Trapper,
            MovementDelay = 400,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Engulf {Duration = 4}}
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
                "flight",
                "flight control",
                "camouflage",
                "animal body",
                "eyelessness",
                "headlessness",
                "limblessness",
                "clinginess"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {"stealthiness", 3},
                {"size", 8},
                {"physical deflection", 17},
                {"weight", 800}
            },
            InitialLevel = 10,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Behavior = MonsterBehavior.Stalking
        };
    }
}
using System.Collections.Generic;
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
            Weight = 800,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Engulf {Duration = 5}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnDigestion,
                        Action = AbilityAction.Digestion,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Suffocate()}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "camouflage",
                    "animal body",
                    "invisibility detection",
                    "eyelessness",
                    "headlessness",
                    "limblessness"
                },
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"stealthiness", 3},
                    {"largeness", Size.Large},
                    {"physical deflection", 17}
                },
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Behavior = MonsterBehavior.Stalking
        };
    }
}
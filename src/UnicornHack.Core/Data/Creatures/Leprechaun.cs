using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Leprechaun = new CreatureVariant
        {
            Name = "leprechaun",
            Species = Species.Leprechaun,
            SpeciesClass = SpeciesClass.Fey,
            MovementDelay = 80,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new StealGold()}
                }
            },
            SimpleProperties = new HashSet<string> {"teleportation", "infravisibility"},
            ValuedProperties = new Dictionary<string, object>
            {
                {"size", 1},
                {"physical deflection", 12},
                {"magic resistance", 20},
                {"weight", 60}
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Behavior = MonsterBehavior.GoldCollector,
            Noise = ActorNoiseType.Laugh
        };
    }
}
using System.Collections.Generic;
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
            Weight = 60,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new StealGold()}
                    }
                },
            SimpleProperties = new HashSet<string> {"teleportation", "infravisibility"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"largeness", Size.Tiny},
                    {"physical deflection", 12},
                    {"magic resistance", 20}
                },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Behavior = MonsterBehavior.GoldCollector,
            Noise = ActorNoiseType.Laugh
        };
    }
}
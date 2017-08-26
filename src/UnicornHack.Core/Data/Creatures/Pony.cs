using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Pony = new CreatureVariant
        {
            Name = "pony",
            Species = Species.Horse,
            SpeciesClass = SpeciesClass.Quadrupedal,
            MovementDelay = 75,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Kick,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 3}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 14}, {"weight", 1300}},
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 7F},
            NextStageName = "horse",
            Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Mountable | MonsterBehavior.Wandering,
            Noise = ActorNoiseType.Neigh
        };
    }
}
using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Woodchuck = new CreatureVariant
        {
            Name = "woodchuck",
            Species = Species.Woodchuck,
            SpeciesClass = SpeciesClass.Rodent,
            MovementDelay = 400,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 3}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "swimming",
                    "animal body",
                    "infravisibility",
                    "handlessness",
                    "singular inventory"
                },
            ValuedProperties =
                new Dictionary<string, object> {{"size", 2}, {"physical deflection", 15}, {"weight", 100}},
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector
        };
    }
}
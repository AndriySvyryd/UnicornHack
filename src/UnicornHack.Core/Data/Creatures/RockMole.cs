using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant RockMole = new CreatureVariant
        {
            Name = "rock mole",
            Species = Species.Mole,
            SpeciesClass = SpeciesClass.Rodent,
            MovementDelay = 400,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "tunneling",
                "animal body",
                "infravisibility",
                "handlessness",
                "singular inventory"
            },
            ValuedProperties = new Dictionary<string, object> {{"size", 2}, {"magic resistance", 20}, {"weight", 100}},
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector
        };
    }
}
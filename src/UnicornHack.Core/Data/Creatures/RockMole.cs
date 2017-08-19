using System.Collections.Generic;
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
            Weight = 100,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
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
                    "tunneling",
                    "animal body",
                    "infravisibility",
                    "handlessness",
                    "singular inventory"
                },
            ValuedProperties = new Dictionary<string, object> {{"largeness", Size.Small}, {"magic resistance", 20}},
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector
        };
    }
}
using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant VampireBat = new CreatureVariant
        {
            Name = "vampire bat",
            Species = Species.Bat,
            SpeciesClass = SpeciesClass.Bird,
            MovementDelay = 60,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnTarget,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<int> {PropertyName = "strength", Value = -1, Duration = 5}
                    }
                }
            },
            SimpleProperties = new HashSet<string>
            {
                "flight",
                "flight control",
                "infravisibility",
                "animal body",
                "handlessness",
                "singular inventory"
            },
            ValuedProperties = new Dictionary<string, object>
            {
                {"poison resistance", 75},
                {"regeneration", 3},
                {"stealthiness", 3},
                {"size", 1},
                {"physical deflection", 14},
                {"weight", 100}
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            PreviousStageName = "giant bat",
            Behavior = MonsterBehavior.Wandering,
            Noise = ActorNoiseType.Sqeek
        };
    }
}
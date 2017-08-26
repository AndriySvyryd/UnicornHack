using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant WaterNymph = new CreatureVariant
        {
            Name = "water nymph",
            Species = Species.Nymph,
            SpeciesClass = SpeciesClass.Fey,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Sedate{Duration = 2}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new StealItem()}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"teleportation", "swimming", "humanoidness", "infravisibility", "femaleness"},
            ValuedProperties =
                new Dictionary<string, object> {{"physical deflection", 11}, {"magic resistance", 20}, {"weight", 600}},
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Behavior = MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Seduction
        };
    }
}
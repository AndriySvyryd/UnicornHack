using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Disenchanter = new CreatureVariant
        {
            Name = "disenchanter",
            Species = Species.Disenchanter,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new DrainEnergy {Amount = 5}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new DrainEnergy {Amount = 2}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "infravisibility",
                    "animal body",
                    "handlessness",
                    "singular inventory",
                    "infravision"
                },
            ValuedProperties =
                new Dictionary<string, object> {{"physical deflection", 30}, {"magic absorption", 30}, {"weight", 750}},
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Noise = ActorNoiseType.Growl
        };
    }
}
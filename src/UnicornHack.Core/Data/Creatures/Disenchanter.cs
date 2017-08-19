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
            Weight = 750,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Disenchant()}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeHit,
                        Effects = new HashSet<Effect> {new Disenchant()}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"infravisibility", "animal body", "handlessness", "singular inventory"},
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 30}, {"magic resistance", 30}},
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Noise = ActorNoiseType.Growl
        };
    }
}
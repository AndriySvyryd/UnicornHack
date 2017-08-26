using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Rothe = new CreatureVariant
        {
            Name = "rothe",
            Species = Species.Quadruped,
            SpeciesClass = SpeciesClass.Quadrupedal,
            MovementDelay = 133,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Headbutt,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 4}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    }
                },
            SimpleProperties =
                new HashSet<string>
                {
                    "blindness",
                    "animal body",
                    "infravisibility",
                    "handlessness",
                    "singular inventory"
                },
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 13}, {"weight", 600}},
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Bleat
        };
    }
}
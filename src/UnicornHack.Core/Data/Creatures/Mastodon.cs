using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Mastodon = new CreatureVariant
        {
            Name = "mastodon",
            Species = Species.Quadruped,
            SpeciesClass = SpeciesClass.Quadrupedal,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Headbutt,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 18}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Headbutt,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 18}}
                    }
                },
            SimpleProperties =
                new HashSet<string> {"animal body", "infravisibility", "handlessness", "singular inventory"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"thick hide", 3},
                    {"size", 8},
                    {"physical deflection", 15},
                    {"weight", 3800}
                },
            InitialLevel = 20,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            Noise = ActorNoiseType.Roar
        };
    }
}
using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Ape = new CreatureVariant
        {
            Name = "ape",
            Species = Species.Simian,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 3}}
                    }
                },
            SimpleProperties = new HashSet<string> {"animal body", "infravisibility", "humanoidness"},
            ValuedProperties = new Dictionary<string, object> {{"physical deflection", 14}, {"weight", 1100}},
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            NextStageName = "carnivorous ape",
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Growl
        };
    }
}
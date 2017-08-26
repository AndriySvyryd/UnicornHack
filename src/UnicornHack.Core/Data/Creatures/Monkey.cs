using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Monkey = new CreatureVariant
        {
            Name = "monkey",
            Species = Species.Simian,
            MovementDelay = 66,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Bite,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 2}}
                    },
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Claw,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new StealItem()}
                    }
                },
            SimpleProperties = new HashSet<string> {"animal body", "infravisibility", "humanoidness"},
            ValuedProperties =
                new Dictionary<string, object> {{"size", 2}, {"physical deflection", 14}, {"weight", 100}},
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            Noise = ActorNoiseType.Growl
        };
    }
}
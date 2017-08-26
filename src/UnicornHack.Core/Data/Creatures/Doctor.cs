using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Doctor = new CreatureVariant
        {
            Name = "doctor",
            Species = Species.Human,
            MovementDelay = 200,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Touch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new Heal {Amount = 7}}
                    }
                },
            SimpleProperties = new HashSet<string> {"infravisibility", "humanoidness"},
            ValuedProperties = new Dictionary<string, object> {{"poison resistance", 3}, {"weight", 1000}},
            InitialLevel = 11,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.RangedPeaceful,
            Noise = ActorNoiseType.Doctor
        };
    }
}
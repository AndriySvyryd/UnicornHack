using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Abbot = new CreatureVariant
        {
            Name = "abbot",
            Species = Species.Human,
            MovementDelay = 100,
            Abilities =
                new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 4}}
                    }
                },
            SimpleProperties = new HashSet<string> {"infravisibility", "humanoidness"},
            ValuedProperties =
                new Dictionary<string, object>
                {
                    {"physical deflection", 14},
                    {"magic resistance", 10},
                    {"weight", 1000}
                },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            NextStageName = "monk",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = MonsterBehavior.Peaceful,
            Noise = ActorNoiseType.Speach
        };
    }
}